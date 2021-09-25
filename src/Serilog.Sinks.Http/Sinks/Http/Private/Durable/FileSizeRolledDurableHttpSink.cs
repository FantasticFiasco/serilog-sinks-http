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
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.Private.IO;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class FileSizeRolledDurableHttpSink : ILogEventSink, IDisposable
    {
        private readonly HttpLogShipper shipper;
        private readonly ILogEventSink sink;

        public FileSizeRolledDurableHttpSink(
            string requestUri,
            string bufferBaseFileName,
            long? bufferFileSizeLimitBytes,
            bool bufferFileShared,
            int? retainedBufferFileCountLimit,
            long? logEventLimitBytes,
            int? logEventsInBatchLimit,
            long? batchSizeLimitBytes,
            TimeSpan period,
            ITextFormatter textFormatter,
            IBatchFormatter batchFormatter,
            IHttpClient httpClient)
        {
            shipper = new HttpLogShipper(
                httpClient,
                requestUri,
                new FileSizeRolledBufferFiles(new DirectoryService(), bufferBaseFileName),
                logEventLimitBytes,
                logEventsInBatchLimit,
                batchSizeLimitBytes,
                period,
                batchFormatter);

            sink = new LoggerConfiguration()
                .WriteTo.File(
                    formatter: textFormatter,
                    path: $"{bufferBaseFileName}-.txt",
                    fileSizeLimitBytes: bufferFileSizeLimitBytes,
                    shared: bufferFileShared,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: retainedBufferFileCountLimit)
                .CreateLogger();
        }

        public void Emit(LogEvent logEvent)
        {
            sink.Emit(logEvent);
        }

        public void Dispose()
        {
            (sink as IDisposable)?.Dispose();
            shipper.Dispose();
        }
    }
}
