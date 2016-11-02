// Copyright 2015-2016 Serilog Contributors
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
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Http
{
    /// <summary>
    /// Send log events using HTTP POST over the network.
    /// </summary>
    public sealed class HttpSink : PeriodicBatchingSink
    {
        private readonly string requestUri;
        private readonly ITextFormatter formatter;

        private HttpClient client;

        /// <summary>
        /// The default batch posting limit.
        /// </summary>
        public static readonly int DefaultBatchPostingLimit = 1000;

        /// <summary>
        /// The default period.
        /// </summary>
        public static readonly TimeSpan DefaultPeriod = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSink"/> class.
        /// </summary>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        public HttpSink(
            string requestUri,
            int batchPostingLimit,
            TimeSpan period,
            IFormatProvider formatProvider)
            : base(batchPostingLimit, period)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            if (formatProvider == null)
                throw new ArgumentNullException(nameof(formatProvider));

            this.requestUri = requestUri;

            formatter = new JsonFormatter(formatProvider: formatProvider, renderMessage: true);
            client = new HttpClient();
        }

        #region PeriodicBatchingSink Members

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        /// <param name="events">The events to emit.</param>
        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var payload = FormatPayload(events);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var result = await client.PostAsync(requestUri, content).ConfigureAwait(false);
            if (!result.IsSuccessStatusCode)
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {requestUri}");
        }

        /// <summary>
        /// Free resources held by the sink.
        /// </summary>
        /// <param name="disposing">
        /// If true, called because the object is being disposed; if false, the object is being
        /// disposed from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && client != null)
            {
                client.Dispose();
                client = null;

            }
        }

        #endregion

        private string FormatPayload(IEnumerable<LogEvent> events)
        {
            var payload = new StringWriter();
            payload.Write("{\"events\":[");

            var delimStart = "";

            foreach (var logEvent in events)
            {
                payload.Write(delimStart);
                formatter.Format(logEvent, payload);

                delimStart = ",";
            }

            payload.Write("]}");
            return payload.ToString();
        }
    }
}