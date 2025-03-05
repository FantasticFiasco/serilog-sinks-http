﻿// Copyright 2015-2025 Serilog Contributors
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
using Serilog.Sinks.Http.TextFormatters;

namespace Serilog.Sinks.Http.Private.Durable;

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
        bool flushOnClose,
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
            flushOnClose,
            batchFormatter);

        sink = CreateFileSink(
            bufferBaseFileName,
            bufferFileSizeLimitBytes,
            bufferFileShared,
            retainedBufferFileCountLimit,
            new FileTextFormatter(textFormatter));
    }

    public void Emit(LogEvent logEvent)
    {
        sink.Emit(logEvent);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            (sink as IDisposable)?.Dispose();
            shipper.Dispose();
        }
    }

    private static ILogEventSink CreateFileSink(
        string bufferBaseFileName,
        long? bufferFileSizeLimitBytes,
        bool bufferFileShared,
        int? retainedBufferFileCountLimit,
        ITextFormatter textFormatter)
    {
        return new LoggerConfiguration()
            .WriteTo.File(
                path: $"{bufferBaseFileName}-.txt",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: bufferFileSizeLimitBytes,
                shared: bufferFileShared,
                retainedFileCountLimit: retainedBufferFileCountLimit,
                formatter: textFormatter,
                rollOnFileSizeLimit: true)
            .CreateLogger();
    }
}
