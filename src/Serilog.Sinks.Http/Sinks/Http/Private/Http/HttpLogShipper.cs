﻿// Copyright 2015-2016 Serilog Contributors
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

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Sinks.Http.Private.Time;
using IOFile = System.IO.File;
#if HRESULTS
using System.Runtime.InteropServices;
#endif

namespace Serilog.Sinks.Http.Private.Http
{
	internal class HttpLogShipper : IDisposable
	{
		private static readonly TimeSpan RequiredLevelCheckInterval = TimeSpan.FromMinutes(2);
		private static readonly string ContentType = "application/json";

		private readonly string requestUri;
		private readonly int batchPostingLimit;
		private readonly long? eventBodyLimitBytes;
		private readonly string bookmarkFilename;
		private readonly string logFolder;
		private readonly string candidateSearchPath;
		private readonly ExponentialBackoffConnectionSchedule connectionSchedule;
		private readonly PortableTimer timer;
		private readonly object stateLock = new object();

		private IHttpClient client;
		private DateTime nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);
		private volatile bool unloading;

		public HttpLogShipper(
			IHttpClient client,
			string requestUri,
			string bufferBaseFilename,
			int batchPostingLimit,
			TimeSpan period,
			long? eventBodyLimitBytes)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));
			if (requestUri == null)
				throw new ArgumentNullException(nameof(requestUri));
			if (bufferBaseFilename == null)
				throw new ArgumentNullException(nameof(bufferBaseFilename));
			if (batchPostingLimit <= 0)
				throw new ArgumentException("batchPostingLimit must be 1 or greater", nameof(batchPostingLimit));

			this.client = client;
			this.requestUri = requestUri;
			this.batchPostingLimit = batchPostingLimit;
			this.eventBodyLimitBytes = eventBodyLimitBytes;

			bookmarkFilename = Path.GetFullPath(bufferBaseFilename + ".bookmark");
			logFolder = Path.GetDirectoryName(bookmarkFilename);
			candidateSearchPath = Path.GetFileName(bufferBaseFilename) + "*.json";
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

					// Locking the bookmark ensures that though there may be multiple instances of this
					// class running, only one will ship logs at a time.

					using (var bookmark = IOFile.Open(
						bookmarkFilename,
						FileMode.OpenOrCreate,
						FileAccess.ReadWrite,
						FileShare.Read))
					{
						long nextLineBeginsAtOffset;
						string currentFile;

						TryReadBookmark(bookmark, out nextLineBeginsAtOffset, out currentFile);

						var fileSet = GetFileSet();

						if (currentFile == null || !IOFile.Exists(currentFile))
						{
							nextLineBeginsAtOffset = 0;
							currentFile = fileSet.FirstOrDefault();
						}

						if (currentFile == null)
							continue;

						var payload = ReadPayload(currentFile, ref nextLineBeginsAtOffset, ref count);

						if (count > 0 || nextRequiredLevelCheckUtc < DateTime.UtcNow)
						{
							lock (stateLock)
							{
								nextRequiredLevelCheckUtc = DateTime.UtcNow.Add(RequiredLevelCheckInterval);
							}

							var content = new StringContent(payload, Encoding.UTF8, ContentType);

							var result = await client.PostAsync(requestUri, content).ConfigureAwait(false);
							if (result.IsSuccessStatusCode)
							{
								connectionSchedule.MarkSuccess();

								WriteBookmark(bookmark, nextLineBeginsAtOffset, currentFile);
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
								WriteBookmark(bookmark, 0, fileSet[1]);
							}

							if (fileSet.Length > 2)
							{
								// Once there's a third file waiting to ship, we do our
								// best to move on, though a lock on the current file
								// will delay this.
								IOFile.Delete(fileSet[0]);
							}
						}
					}
				}
				while (count == batchPostingLimit);
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

		private string ReadPayload(string currentFile, ref long nextLineBeginsAtOffset, ref int count)
		{
			var payload = new StringWriter();
			payload.Write("{\"events\":[");
			var delimStart = "";

			using (var current = IOFile.Open(currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				current.Position = nextLineBeginsAtOffset;

				string nextLine;
				while (count < batchPostingLimit &&
					   TryReadLine(current, ref nextLineBeginsAtOffset, out nextLine))
				{
					// Count is the indicator that work was done, so advances even in the (rare) case an
					// oversized event is dropped
					count++;

					if (eventBodyLimitBytes.HasValue && Encoding.UTF8.GetByteCount(nextLine) > eventBodyLimitBytes.Value)
					{
						SelfLog.WriteLine(
							"Event JSON representation exceeds the byte size limit of {0} and will be dropped; data: {1}",
							eventBodyLimitBytes, nextLine);
					}
					else
					{
						payload.Write(delimStart);
						payload.Write(nextLine);
						delimStart = ",";
					}
				}

				payload.Write("]}");
			}
			return payload.ToString();
		}

		private static bool IsUnlockedAtLength(string file, long maxLength)
		{
			try
			{
				using (var fileStream = IOFile.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
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

		private static void WriteBookmark(FileStream bookmark, long nextLineBeginsAtOffset, string currentFile)
		{
			using (var writer = new StreamWriter(bookmark))
			{
				writer.WriteLine("{0}:::{1}", nextLineBeginsAtOffset, currentFile);
			}
		}

		// It would be ideal to chomp whitespace here, but not required
		private static bool TryReadLine(Stream current, ref long nextStart, out string nextLine)
		{
			var includesBom = nextStart == 0;

			if (current.Length <= nextStart)
			{
				nextLine = null;
				return false;
			}

			current.Position = nextStart;

			// Important not to dispose this StreamReader as the stream must remain open.
			var reader = new StreamReader(current, Encoding.UTF8, false, 128);
			nextLine = reader.ReadLine();

			if (nextLine == null)
				return false;

			nextStart += Encoding.UTF8.GetByteCount(nextLine) + Encoding.UTF8.GetByteCount(Environment.NewLine);
			if (includesBom)
				nextStart += 3;

			return true;
		}

		private static void TryReadBookmark(Stream bookmark, out long nextLineBeginsAtOffset, out string currentFile)
		{
			nextLineBeginsAtOffset = 0;
			currentFile = null;

			if (bookmark.Length != 0)
			{
				// Important not to dispose this StreamReader as the stream must remain open.
				var reader = new StreamReader(bookmark, Encoding.UTF8, false, 128);
				var current = reader.ReadLine();

				if (current != null)
				{
					bookmark.Position = 0;
					var parts = current.Split(new[] { ":::" }, StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length == 2)
					{
						nextLineBeginsAtOffset = long.Parse(parts[0]);
						currentFile = parts[1];
					}
				}

			}
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