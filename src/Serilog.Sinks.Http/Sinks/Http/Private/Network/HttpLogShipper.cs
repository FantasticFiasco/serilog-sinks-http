﻿// Copyright 2015-2018 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Sinks.Http.Private.Time;
#if HRESULTS
using System.Runtime.InteropServices;
#endif

namespace Serilog.Sinks.Http.Private.Network
{
    public class HttpLogShipper : IDisposable
    {
        private const string ContentType = "application/json";

        private static readonly TimeSpan RequiredLevelCheckInterval = TimeSpan.FromMinutes(2);
        private static readonly Regex BufferPathFormatRegex = new Regex(
            $"(?<prefix>.+)(?:{string.Join("|", Enum.GetNames(typeof(DateFormats)).Select(x => $"{{{x}}}"))})(?<postfix>.+)");

        private readonly string requestUri;
        private readonly int batchPostingLimit;
        private readonly string bookmarkFilename;
        private readonly string logFolder;
        private readonly string candidateSearchPath;
        private readonly ExponentialBackoffConnectionSchedule connectionSchedule;
        private readonly PortableTimer timer;
        private readonly object stateLock = new object();
        private readonly IBatchFormatter batchFormatter;
        private IHttpClient client;
        private DateTime nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);
        private volatile bool unloading;

        public HttpLogShipper(
            IHttpClient client,
            string requestUri,
            string bufferPathFormat,
            int batchPostingLimit,
            TimeSpan period,
            IBatchFormatter batchFormatter)
        {
            if (bufferPathFormat == null) throw new ArgumentNullException(nameof(bufferPathFormat));
            if (bufferPathFormat != bufferPathFormat.Trim()) throw new ArgumentException("bufferPathFormat must not contain any leading or trailing whitespaces", nameof(bufferPathFormat));
            if (batchPostingLimit <= 0) throw new ArgumentException("batchPostingLimit must be 1 or greater", nameof(batchPostingLimit));

            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            this.batchPostingLimit = batchPostingLimit;
            this.batchFormatter = batchFormatter ?? throw new ArgumentNullException(nameof(batchFormatter));

            var bufferPathFormatMatch = BufferPathFormatRegex.Match(bufferPathFormat);
            if (!bufferPathFormatMatch.Success)
            {
                throw new ArgumentException($"bufferPathFormat must include one of the date formats [{string.Join(", ", Enum.GetNames(typeof(DateFormats)))}]");
            }

            var prefix = bufferPathFormatMatch.Groups["prefix"];
            var postfix = bufferPathFormatMatch.Groups["postfix"];

            bookmarkFilename = Path.GetFullPath(prefix.Value.TrimEnd('-') + ".bookmark");
            logFolder = Path.GetDirectoryName(bookmarkFilename);
            candidateSearchPath = $"{Path.GetFileName(prefix.Value)}*{postfix.Value}";
            connectionSchedule = new ExponentialBackoffConnectionSchedule(period);
            timer = new PortableTimer(OnTick);

            SetTimer();
        }

        public void Dispose()
        {
            CloseAndFlush();

            client?.Dispose();
            client = null;
        }

        private void SetTimer()
        {
            // Note, called under stateLock
            timer.Start(connectionSchedule.NextInterval);
        }

        private async Task OnTick()
        {
            try
            {
                int count;

                do
                {
                    count = 0;

                    using (var bookmark = new BookmarkFile(bookmarkFilename))
                    {
                        bookmark.TryReadBookmark(out var nextLineBeginsAtOffset, out var currentFile);

                        var fileSet = GetFileSet();

                        if (currentFile == null || !System.IO.File.Exists(currentFile))
                        {
                            nextLineBeginsAtOffset = 0;
                            currentFile = fileSet.FirstOrDefault();
                        }

                        if (currentFile == null)
                            continue;

                        var logEvents = PayloadReader.Read(
                            currentFile,
                            ref nextLineBeginsAtOffset,
                            ref count,
                            batchPostingLimit);

                        var payloadWriter = new StringWriter();
                        batchFormatter.Format(logEvents, payloadWriter);
                        var payload = payloadWriter.ToString();
                        
                        if (count > 0 || nextRequiredLevelCheckUtc < DateTime.UtcNow)
                        {
                            lock (stateLock)
                            {
                                nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);
                            }

                            if (string.IsNullOrEmpty(payload))
                                continue;

                            var content = new StringContent(payload, Encoding.UTF8, ContentType);

                            var result = await client.PostAsync(requestUri, content).ConfigureAwait(false);
                            if (result.IsSuccessStatusCode)
                            {
                                connectionSchedule.MarkSuccess();

                                bookmark.WriteBookmark(nextLineBeginsAtOffset, currentFile);
                            }
                            else
                            {
                                connectionSchedule.MarkFailure();

                                SelfLog.WriteLine(
                                    "Received failed HTTP shipping result {0}: {1}",
                                    result.StatusCode,
                                    await result.Content.ReadAsStringAsync().ConfigureAwait(false));

                                break;
                            }
                        }
                        else
                        {
                            // For whatever reason, there's nothing waiting to send. This means we should try connecting
                            // again at the regular interval, so mark the attempt as successful.
                            connectionSchedule.MarkSuccess();

                            // Only advance the bookmark if no other process has the
                            // current file locked, and its length is as we found it.
                            if (fileSet.Length == 2 &&
                                fileSet.First() == currentFile &&
                                IsUnlockedAtLength(currentFile, nextLineBeginsAtOffset))
                            {
                                bookmark.WriteBookmark(0, fileSet[1]);
                            }

                            if (fileSet.Length > 2)
                            {
                                // Once there's a third file waiting to ship, we do our
                                // best to move on, though a lock on the current file
                                // will delay this.
                                System.IO.File.Delete(fileSet[0]);
                            }
                        }
                    }
                } while (count == batchPostingLimit);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, e);
                connectionSchedule.MarkFailure();
            }
            finally
            {
                lock (stateLock)
                {
                    if (!unloading)
                    {
                        SetTimer();
                    }
                }
            }
        }

        private static bool IsUnlockedAtLength(string file, long maxLength)
        {
            try
            {
                using (var fileStream = System.IO.File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    return fileStream.Length <= maxLength;
                }
            }
#if HRESULTS
            catch (IOException ex)
            {
                var errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
                if (errorCode != 32 && errorCode != 33)
                {
                    SelfLog.WriteLine("Unexpected I/O exception while testing locked status of {0}: {1}", file, ex);
                }
            }
#else
            catch (IOException)
            {
                // Where no HRESULT is available, assume IOExceptions indicate a locked file
            }
#endif
            catch (Exception ex)
            {
                SelfLog.WriteLine("Unexpected exception while testing locked status of {0}: {1}", file, ex);
            }

            return false;
        }

        private string[] GetFileSet()
        {
            return Directory.GetFiles(logFolder, candidateSearchPath)
                .OrderBy(file => file)
                .ToArray();
        }

        private void CloseAndFlush()
        {
            lock (stateLock)
            {
                if (unloading)
                    return;

                unloading = true;
            }

            timer.Dispose();

            OnTick().GetAwaiter().GetResult();
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
