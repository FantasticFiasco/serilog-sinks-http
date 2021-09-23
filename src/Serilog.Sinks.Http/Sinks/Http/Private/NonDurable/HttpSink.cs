// Copyright 2015-2020 Serilog Contributors
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
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.Private.Time;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class HttpSink : ILogEventSink, IDisposable
    {
        private readonly string requestUri;
        private readonly long? logEventLimitBytes;
        private readonly int? logEventsInBatchLimit;
        private readonly long? batchSizeLimitBytes;
        private readonly ITextFormatter textFormatter;
        private readonly IBatchFormatter batchFormatter;
        private readonly IHttpClient httpClient;
        private readonly ExponentialBackoffConnectionSchedule connectionSchedule;
        private readonly PortableTimer timer;
        private readonly object syncRoot = new();
        private readonly LogEventQueue queue;

        private Batch? unsentBatch;
        private volatile bool disposed;

        public HttpSink(
            string requestUri,
            long? logEventLimitBytes,
            int? logEventsInBatchLimit,
            long? batchSizeLimitBytes,
            int? queueLimit,
            TimeSpan period,
            ITextFormatter textFormatter,
            IBatchFormatter batchFormatter,
            IHttpClient httpClient)
        {
            this.requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            this.logEventLimitBytes = logEventLimitBytes;
            this.logEventsInBatchLimit = logEventsInBatchLimit;
            this.batchSizeLimitBytes = batchSizeLimitBytes;
            this.textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            this.batchFormatter = batchFormatter ?? throw new ArgumentNullException(nameof(batchFormatter));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            connectionSchedule = new ExponentialBackoffConnectionSchedule(period);
            timer = new PortableTimer(OnTick);
            queue = new LogEventQueue(queueLimit);

            SetTimer();
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var writer = new StringWriter();
            textFormatter.Format(logEvent, writer);
            var formattedLogEvent = writer.ToString();

            if (ByteSize.From(formattedLogEvent) > logEventLimitBytes)
            {
                SelfLog.WriteLine(
                    "Log event exceeds the size limit of {0} bytes set for this sink and will be dropped; data: {1}",
                    logEventLimitBytes,
                    formattedLogEvent);
                return;
            }

            var result = queue.TryEnqueue(formattedLogEvent);
            if (result == LogEventQueue.EnqueueResult.QueueFull)
            {
                SelfLog.WriteLine("Queue has reached its limit and the log event will be dropped; data: {0}", formattedLogEvent);
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (disposed)
                    return;

                disposed = true;
            }

            timer.Dispose();

            OnTick().GetAwaiter().GetResult();
            httpClient.Dispose();
        }

        private void SetTimer()
        {
            // Note, called under syncRoot
            timer.Start(connectionSchedule.NextInterval);
        }

        private async Task OnTick()
        {
            try
            {
                Batch? batch;

                do
                {
                    batch = unsentBatch ?? LogEventQueueReader.Read(queue, logEventsInBatchLimit, batchSizeLimitBytes);

                    if (batch.LogEvents.Count > 0)
                    {
                        HttpResponseMessage response;

                        using (var contentStream = new MemoryStream())
                        using (var contentWriter = new StreamWriter(contentStream, Encoding.UTF8WithoutBom))
                        {
                            batchFormatter.Format(batch.LogEvents, contentWriter);

                            await contentWriter.FlushAsync();
                            contentStream.Position = 0;

                            if (contentStream.Length == 0)
                                continue;

                            response = await httpClient
                                .PostAsync(requestUri, contentStream)
                                .ConfigureAwait(false);
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            connectionSchedule.MarkSuccess();
                            unsentBatch = null;
                        }
                        else
                        {
                            connectionSchedule.MarkFailure();
                            unsentBatch = batch;

                            var statusCode = response.StatusCode;
                            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            SelfLog.WriteLine("Received failed HTTP shipping result {0}: {1}", statusCode, body);
                            break;
                        }
                    }
                    else
                    {
                        // For whatever reason, there's nothing waiting to be sent. This means we should try connecting
                        // again at the regular interval, so mark the attempt as successful.
                        connectionSchedule.MarkSuccess();
                    }
                } while (batch.HasReachedLimit);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, e);
                connectionSchedule.MarkFailure();
            }
            finally
            {
                lock (syncRoot)
                {
                    if (!disposed)
                    {
                        SetTimer();
                    }
                }
            }
        }
    }
}
