﻿// Copyright 2015-2020 Serilog Contributors
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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.Private.Time;

namespace Serilog.Sinks.Http.Private.Sinks.NonDurable
{
    /// <summary>
    /// A non-durable sink that sends log events using HTTP POST over the network. A non-durable
    /// sink will lose data after a system or process restart.
    /// </summary>
    public class HttpSink : ILogEventSink, IDisposable
    {
        private const string ContentType = "application/json";

        private readonly string requestUri;
        private readonly int batchPostingLimit;
        private readonly long batchSizeLimitBytes;
        private readonly ITextFormatter textFormatter;
        private readonly IBatchFormatter batchFormatter;
        private readonly IHttpClient httpClient;
        private readonly ExponentialBackoffConnectionSchedule connectionSchedule;
        private readonly PortableTimer timer;
        private readonly LogEventQueue<LogEvent> queue;

        private Batch currentBatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSink"/> class.
        /// </summary>
        public HttpSink(
            string requestUri,
            int batchPostingLimit,
            long batchSizeLimitBytes,
            int? queueLimit,
            TimeSpan period,
            ITextFormatter textFormatter,
            IBatchFormatter batchFormatter,
            IHttpClient httpClient)
        {
            this.requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
            this.batchPostingLimit = batchPostingLimit;
            this.batchSizeLimitBytes = batchSizeLimitBytes;
            this.textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            this.batchFormatter = batchFormatter ?? throw new ArgumentNullException(nameof(batchFormatter));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            connectionSchedule = new ExponentialBackoffConnectionSchedule(period);
            timer = new PortableTimer(OnTick);
            queue = new LogEventQueue<LogEvent>(queueLimit);

            SetTimer(); 
        }

        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var success = queue.TryEnqueue(logEvent);
            if (!success)
            {
                SelfLog.WriteLine("Queue has reached its limit and the log event will be dropped");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            timer?.Dispose();
            httpClient?.Dispose();
        }

        private void SetTimer()
        {
            // Note, called under syncRoot
            timer.Start(connectionSchedule.NextInterval);
        }

        // /// <inheritdoc />
        // public async Task EmitBatchAsync(IEnumerable<LogEvent> logEvents)
        // {
        //     var payload = FormatPayload(logEvents);
        //     var content = new StringContent(payload, Encoding.UTF8, ContentType);

        //     var result = await httpClient
        //         .PostAsync(requestUri, content)
        //         .ConfigureAwait(false);

        //     if (!result.IsSuccessStatusCode)
        //     {
        //         throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {requestUri}");
        //     }
        // }

        private async Task OnTick()
        {
            try
            {
                do
                {
                    if (currentBatch == null)
                    {
                        currentBatch = new Batch();
                        while (currentBatch.LogEvents.Count < batchPostingLimit
                            && queue.TryDequeue(out var logEvent))
                        {
                            currentBatch.LogEvents.Add(logEvent);
                        }
                    }

                    if (currentBatch.LogEvents.Count == 0)
                    {
                        currentBatch = null;
                        return;
                    }

                    var payload = FormatPayload(currentBatch.LogEvents);
                    var content = new StringContent(payload, Encoding.UTF8, ContentType);

                    var result = await httpClient
                        .PostAsync(requestUri, content)
                        .ConfigureAwait(false);

                    if (!result.IsSuccessStatusCode)
                    {
                        throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {requestUri}");
                    }

                    connectionSchedule.MarkSuccess();
                } while (currentBatch != null && currentBatch.HasReachedLimit);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, e);
                connectionSchedule.MarkFailure();
            }
            
        }

        private string FormatPayload(IEnumerable<LogEvent> logEvents)
        {
            var payload = new StringWriter();

            batchFormatter.Format(logEvents, textFormatter, payload);

            return payload.ToString();
        }
    }
}