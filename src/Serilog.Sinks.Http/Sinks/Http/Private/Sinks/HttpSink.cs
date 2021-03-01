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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Http.Private.Sinks
{
    /// <summary>
    /// A non-durable sink that sends log events using HTTP POST over the network. A non-durable
    /// sink will lose data after a system or process restart.
    /// </summary>
    /// <seealso cref="PeriodicBatchingSink" />
    public class HttpSink : ILogEventSink, IBatchedLogEventSink, IDisposable
    {
        private const string ContentType = "application/json";

        private readonly string requestUri;
        private readonly ITextFormatter textFormatter;
        private readonly IBatchFormatter batchFormatter;
        private readonly PeriodicBatchingSink sink;

        private IHttpClient httpClient;

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
            this.textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
            this.batchFormatter = batchFormatter ?? throw new ArgumentNullException(nameof(batchFormatter));
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            sink = new PeriodicBatchingSink(this, new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = batchPostingLimit,
                Period = period,
                QueueLimit = queueLimit
            });
        }

        /// <inheritdoc />
        public async Task EmitBatchAsync(IEnumerable<LogEvent> logEvents)
        {
            var payload = FormatPayload(logEvents);
            var content = new StringContent(payload, Encoding.UTF8, ContentType);

            var result = await httpClient
                .PostAsync(requestUri, content)
                .ConfigureAwait(false);

            if (!result.IsSuccessStatusCode)
            {
                throw new LoggingFailedException($"Received failed result {result.StatusCode} when posting events to {requestUri}");
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                httpClient?.Dispose();
                httpClient = null;
            }
        }

        private string FormatPayload(IEnumerable<LogEvent> logEvents)
        {
            var payload = new StringWriter();

            batchFormatter.Format(logEvents, textFormatter, payload);

            return payload.ToString();
        }

        public void Emit(LogEvent logEvent) => throw new NotImplementedException();

        Task IBatchedLogEventSink.EmitBatchAsync(IEnumerable<LogEvent> batch) => throw new NotImplementedException();

        public Task OnEmptyBatchAsync() => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();
    }
}
