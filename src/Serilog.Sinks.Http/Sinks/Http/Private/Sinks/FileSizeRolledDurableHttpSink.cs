// Copyright 2015-2018 Serilog Contributors
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
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.Private.IO;
using Serilog.Sinks.Http.Private.Network;

namespace Serilog.Sinks.Http.Private.Sinks
{
    /// <summary>
    /// A durable sink that sends log events using HTTP POST over the network. A durable
    /// sink will persist log events on disk in buffer files before sending them over the
    /// network, thus protecting against data loss after a system or process restart. The
    /// buffer files will use a rolling behavior based on file size.
    /// </summary>
    /// <seealso cref="ILogEventSink" />
    /// <seealso cref="IDisposable" />
    public class FileSizeRolledDurableHttpSink : ILogEventSink, IDisposable
    {
        private readonly HttpLogShipper shipper;
        private readonly ILogEventSink sink;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSizeRolledDurableHttpSink"/> class.
        /// </summary>
        public FileSizeRolledDurableHttpSink(
            string requestUri,
            string bufferBaseFileName,
            long? bufferFileSizeLimitBytes,
            int? retainedBufferFileCountLimit,
            int batchPostingLimit,
            TimeSpan period,
            ITextFormatter textFormatter,
            IBatchFormatter batchFormatter,
            IHttpClient client)
        {
            if (bufferFileSizeLimitBytes.HasValue && bufferFileSizeLimitBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferFileSizeLimitBytes), "Negative value provided; file size limit must be non-negative.");

            shipper = new HttpLogShipper(
                client,
                requestUri,
                new FileSizeRolledBufferFiles(new DirectoryService(), bufferBaseFileName),
                batchPostingLimit,
                period,
                batchFormatter);

            sink = new LoggerConfiguration()
                .WriteTo.File(
                    textFormatter,
                    $"{bufferBaseFileName}-.json",
                    fileSizeLimitBytes: bufferFileSizeLimitBytes,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedBufferFileCountLimit,
                    rollingInterval: RollingInterval.Day,
                    encoding: Encoding.UTF8)
                .CreateLogger();
        }

        /// <inheritdoc />
        public void Emit(LogEvent logEvent) =>
            sink.Emit(logEvent);

        /// <inheritdoc />
        public void Dispose()
        {
            (sink as IDisposable)?.Dispose();
            shipper?.Dispose();
        }
    }
}
