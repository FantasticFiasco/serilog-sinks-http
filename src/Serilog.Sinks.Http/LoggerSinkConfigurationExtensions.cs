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
using Microsoft.Extensions.Configuration;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.Private.Durable;
using Serilog.Sinks.Http.Private.NonDurable;
using Serilog.Sinks.Http.TextFormatters;

// <param name="eventBodyLimitBytes">
// The maximum size, in bytes, that the JSON representation of an event may take before it
// is dropped rather than being sent to the server. Specify null for no limit.
// </param>

// long? eventBodyLimitBytes

// <summary>
// Checks the size of the log event body.
// </summary>
// <returns>true if body size is within acceptable range; otherwise false.</returns>
//protected bool CheckEventBodySize(string json)
//{
//    if (eventBodyLimitBytes.HasValue
//         && ByteSize.From(json) > eventBodyLimitBytes.Value)
//     {
//         SelfLog.WriteLine(
//             "Event JSON representation exceeds the size limit of {0} bytes set for this sink and will be dropped; data: {1}",
//             eventBodyLimitBytes,
//             json);
//
//         return false;
//     }
//
//     return true;
// }

// <param name="eventBodyLimitBytes">
// The maximum size, in bytes, that the JSON representation of an event may take before it
// is dropped rather than being sent to the server. Specify null for no limit. Default
// value is 256 KB.
// </param>

// long? eventBodyLimitBytes = 256 * ByteSize.KB

namespace Serilog
{
    /// <summary>
    /// Class containing extension methods to <see cref="LoggerConfiguration"/>, configuring sinks
    /// sending log events over the network using HTTP.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Adds a non-durable sink that sends log events using HTTP POST over the network. The log
        /// events are stored in memory in the case that the log server cannot be reached.
        /// <para/>
        /// The maximum number of log events stored in memory is configurable, and given that we
        /// reach this limit the sink will drop new log events in favor of keeping the old.
        /// <para/>
        /// A non-durable sink will lose data after a system or process restart.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="batchSizeLimitBytes">
        /// The approximate maximum size, in bytes, for a single batch. The value is an
        /// approximation because only the size of the log events are considered. The extra
        /// characters added by the batch formatter, where the sequence of serialized log events
        /// are transformed into a payload, are not considered. Please make sure to accommodate for
        /// those.
        /// <para/>
        /// Another thing to mention is that although the sink does its best to optimize for this
        /// limit, if you decide to use an implementation of <seealso cref="IHttpClient"/> that is
        /// compressing the payload, e.g. <seealso cref="JsonGzipHttpClient"/>, this parameter
        /// describes the uncompressed size of the log events. The compressed size might be
        /// significantly smaller depending on the compression algorithm and the repetitiveness of
        /// the log events.
        /// <para/>
        /// Default value is long.MaxValue.
        /// </param>
        /// <param name="queueLimit">
        /// The maximum number of events stored in the queue in memory, waiting to be posted over
        /// the network. Default value is infinitely.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="textFormatter">
        /// The formatter rendering individual log events into text, for example JSON. Default
        /// value is <see cref="NormalRenderedTextFormatter"/>.
        /// </param>
        /// <param name="batchFormatter">
        /// The formatter batching multiple log events into a payload that can be sent over the
        /// network. Default value is <see cref="DefaultBatchFormatter"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation. Default value is
        /// <see cref="JsonHttpClient"/>.
        /// </param>
        /// <param name="configuration">
        /// Configuration passed to <paramref name="httpClient"/>. Parameter is either manually
        /// specified when configuring the sink in source code or automatically passed in when
        /// configuring the sink using
        /// <see href="https://www.nuget.org/packages/Serilog.Settings.Configuration">Serilog.Settings.Configuration</see>.
        /// </param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration Http(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            int batchPostingLimit = 1000,
            long batchSizeLimitBytes = long.MaxValue,
            int? queueLimit = null,
            TimeSpan? period = null,
            ITextFormatter? textFormatter = null,
            IBatchFormatter? batchFormatter = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient? httpClient = null,
            IConfiguration? configuration = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (requestUri == null) throw new ArgumentNullException(nameof(requestUri));

            // Default values
            period ??= TimeSpan.FromSeconds(2);
            textFormatter ??= new NormalRenderedTextFormatter();
            batchFormatter ??= new DefaultBatchFormatter();
            httpClient ??= new JsonHttpClient();

            if (configuration != null)
            {
                httpClient.Configure(configuration);
            }

            var sink = new HttpSink(
                requestUri: requestUri,
                batchPostingLimit: batchPostingLimit,
                batchSizeLimitBytes: batchSizeLimitBytes,
                queueLimit: queueLimit,
                period: period.Value,
                textFormatter: textFormatter,
                batchFormatter: batchFormatter,
                httpClient: httpClient);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        /// <summary>
        /// Adds a durable sink that sends log events using HTTP POST over the network. The log
        /// events are always stored on disk in the case that the log server cannot be reached.
        /// <para/>
        /// The buffer files will use a rolling behavior defined by the file size specified in
        /// <paramref name="bufferFileSizeLimitBytes"/>, i.e. a new buffer file is created when the
        /// current buffer file has reached its limit. The maximum number of retained files is
        /// defined by <paramref name="retainedBufferFileCountLimit"/>, and when that limit is
        /// reached the oldest file is dropped to make room for a new.
        /// <para/>
        /// A durable sink will protect you against data loss after a system or process restart.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="bufferBaseFileName">
        /// The relative or absolute path for a set of files that will be used to buffer events
        /// until they can be successfully transmitted across the network. Individual files will be
        /// created using the pattern "<paramref name="bufferBaseFileName"/>-*.txt", which should
        /// not clash with any other file names in the same directory. Default value is "Buffer".
        /// </param>
        /// <param name="bufferFileSizeLimitBytes">
        /// The approximate maximum size, in bytes, to which a buffer file will be allowed to grow.
        /// For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial
        /// events, the last event within the limit will be written in full even if it exceeds the
        /// limit.
        /// </param>
        /// <param name="bufferFileShared">
        /// Allow the buffer file to be shared by multiple processes. Default value is false.
        /// </param>
        /// <param name="retainedBufferFileCountLimit">
        /// The maximum number of buffer files that will be retained, including the current buffer
        /// file. Under normal operation only 2 files will be kept, however if the log server is
        /// unreachable, the number of files specified by
        /// <paramref name="retainedBufferFileCountLimit"/> will be kept on the file system. For
        /// unlimited retention, pass null. Default value is 31.
        /// </param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="batchSizeLimitBytes">
        /// The approximate maximum size, in bytes, for a single batch. The value is an
        /// approximation because only the size of the log events are considered. The extra
        /// characters added by the batch formatter, where the sequence of serialized log events
        /// are transformed into a payload, are not considered. Please make sure to accommodate for
        /// those.
        /// <para/>
        /// Another thing to mention is that although the sink does its best to optimize for this
        /// limit, if you decide to use an implementation of <seealso cref="IHttpClient"/> that is
        /// compressing the payload, e.g. <seealso cref="JsonGzipHttpClient"/>, this parameter
        /// describes the uncompressed size of the log events. The compressed size might be
        /// significantly smaller depending on the compression algorithm and the repetitiveness of
        /// the log events.
        /// <para/>
        /// Default value is long.MaxValue.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="textFormatter">
        /// The formatter rendering individual log events into text, for example JSON. Default
        /// value is <see cref="NormalRenderedTextFormatter"/>.
        /// </param>
        /// <param name="batchFormatter">
        /// The formatter batching multiple log events into a payload that can be sent over the
        /// network. Default value is <see cref="DefaultBatchFormatter"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation. Default value is
        /// <see cref="JsonHttpClient"/>.
        /// </param>
        /// <param name="configuration">
        /// Configuration passed to <paramref name="httpClient"/>. Parameter is either manually
        /// specified when configuring the sink in source code or automatically passed in when
        /// configuring the sink using
        /// <see href="https://www.nuget.org/packages/Serilog.Settings.Configuration">Serilog.Settings.Configuration</see>.
        /// </param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration DurableHttpUsingFileSizeRolledBuffers(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            string bufferBaseFileName = "Buffer",
            long? bufferFileSizeLimitBytes = ByteSize.GB,
            bool bufferFileShared = false,
            int? retainedBufferFileCountLimit = 31,
            int batchPostingLimit = 1000,
            long batchSizeLimitBytes = long.MaxValue,
            TimeSpan? period = null,
            ITextFormatter? textFormatter = null,
            IBatchFormatter? batchFormatter = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient? httpClient = null,
            IConfiguration? configuration = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            // Default values
            period ??= TimeSpan.FromSeconds(2);
            textFormatter ??= new NormalRenderedTextFormatter();
            batchFormatter ??= new DefaultBatchFormatter();
            httpClient ??= new JsonHttpClient();

            if (configuration != null)
            {
                httpClient.Configure(configuration);
            }

            var sink = new FileSizeRolledDurableHttpSink(
                requestUri: requestUri,
                bufferBaseFileName: bufferBaseFileName,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: bufferFileShared,
                retainedBufferFileCountLimit: retainedBufferFileCountLimit,
                batchPostingLimit: batchPostingLimit,
                batchSizeLimitBytes: batchSizeLimitBytes,
                period: period.Value,
                textFormatter: textFormatter,
                batchFormatter: batchFormatter,
                httpClient: httpClient);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        /// <summary>
        /// Adds a durable sink that sends log events using HTTP POST over the network. The log
        /// events are always stored on disk in the case that the log server cannot be reached.
        /// <para/>
        /// The buffer files will use a rolling behavior defined by the time interval specified in
        /// <paramref name="bufferRollingInterval"/>, i.e. a new buffer file is created every time
        /// a new interval is started. The maximum size of a buffer file is defined by
        /// <paramref name="bufferFileSizeLimitBytes"/>, and when that limit is reached all new log
        /// events will be dropped until a new interval is started.
        /// <para/>
        /// A durable sink will protect you against data loss after a system or process restart.
        /// </summary>
        /// <param name="sinkConfiguration">The logger configuration.</param>
        /// <param name="requestUri">The URI the request is sent to.</param>
        /// <param name="bufferBaseFileName">
        /// The relative or absolute path for a set of files that will be used to buffer events
        /// until they can be successfully transmitted across the network. Individual files will be
        /// created using the pattern "<paramref name="bufferBaseFileName"/>-*.txt", which should
        /// not clash with any other file names in the same directory. Default value is "Buffer".
        /// </param>
        /// <param name="bufferRollingInterval">
        /// The interval at which the buffer files are rotated. Default value is Day.
        /// </param>
        /// <param name="bufferFileSizeLimitBytes">
        /// The approximate maximum size, in bytes, to which a buffer file for a specific time
        /// interval will be allowed to grow. By default no limit will be applied.
        /// </param>
        /// <param name="bufferFileShared">
        /// Allow the buffer file to be shared by multiple processes. Default value is false.
        /// </param>
        /// <param name="retainedBufferFileCountLimit">
        /// The maximum number of buffer files that will be retained, including the current buffer
        /// file. Under normal operation only 2 files will be kept, however if the log server is
        /// unreachable, the number of files specified by
        /// <paramref name="retainedBufferFileCountLimit"/> will be kept on the file system. For
        /// unlimited retention, pass null. Default value is 31.
        /// </param>
        /// <param name="batchPostingLimit">
        /// The maximum number of events to post in a single batch. Default value is 1000.
        /// </param>
        /// <param name="batchSizeLimitBytes">
        /// The approximate maximum size, in bytes, for a single batch. The value is an
        /// approximation because only the size of the log events are considered. The extra
        /// characters added by the batch formatter, where the sequence of serialized log events
        /// are transformed into a payload, are not considered. Please make sure to accommodate for
        /// those.
        /// <para/>
        /// Another thing to mention is that although the sink does its best to optimize for this
        /// limit, if you decide to use an implementation of <seealso cref="IHttpClient"/> that is
        /// compressing the payload, e.g. <seealso cref="JsonGzipHttpClient"/>, this parameter
        /// describes the uncompressed size of the log events. The compressed size might be
        /// significantly smaller depending on the compression algorithm and the repetitiveness of
        /// the log events.
        /// <para/>
        /// Default value is long.MaxValue.
        /// </param>
        /// <param name="period">
        /// The time to wait between checking for event batches. Default value is 2 seconds.
        /// </param>
        /// <param name="textFormatter">
        /// The formatter rendering individual log events into text, for example JSON. Default
        /// value is <see cref="NormalRenderedTextFormatter"/>.
        /// </param>
        /// <param name="batchFormatter">
        /// The formatter batching multiple log events into a payload that can be sent over the
        /// network. Default value is <see cref="DefaultBatchFormatter"/>.
        /// </param>
        /// <param name="restrictedToMinimumLevel">
        /// The minimum level for events passed through the sink. Default value is
        /// <see cref="LevelAlias.Minimum"/>.
        /// </param>
        /// <param name="httpClient">
        /// A custom <see cref="IHttpClient"/> implementation. Default value is
        /// <see cref="JsonHttpClient"/>.
        /// </param>
        /// <param name="configuration">
        /// Configuration passed to <paramref name="httpClient"/>. Parameter is either manually
        /// specified when configuring the sink in source code or automatically passed in when
        /// configuring the sink using
        /// <see href="https://www.nuget.org/packages/Serilog.Settings.Configuration">Serilog.Settings.Configuration</see>.
        /// </param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        public static LoggerConfiguration DurableHttpUsingTimeRolledBuffers(
            this LoggerSinkConfiguration sinkConfiguration,
            string requestUri,
            string bufferBaseFileName = "Buffer",
            BufferRollingInterval bufferRollingInterval = BufferRollingInterval.Day,
            long? bufferFileSizeLimitBytes = null,
            bool bufferFileShared = false,
            int? retainedBufferFileCountLimit = 31,
            int batchPostingLimit = 1000,
            long batchSizeLimitBytes = long.MaxValue,
            TimeSpan? period = null,
            ITextFormatter? textFormatter = null,
            IBatchFormatter? batchFormatter = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IHttpClient? httpClient = null,
            IConfiguration? configuration = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            // Default values
            period ??= TimeSpan.FromSeconds(2);
            textFormatter ??= new NormalRenderedTextFormatter();
            batchFormatter ??= new DefaultBatchFormatter();
            httpClient ??= new JsonHttpClient();

            if (configuration != null)
            {
                httpClient.Configure(configuration);
            }

            var sink = new TimeRolledDurableHttpSink(
                requestUri: requestUri,
                bufferBaseFileName: bufferBaseFileName,
                bufferRollingInterval: bufferRollingInterval,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: bufferFileShared,
                retainedBufferFileCountLimit: retainedBufferFileCountLimit,
                batchPostingLimit: batchPostingLimit,
                batchSizeLimitBytes: batchSizeLimitBytes,
                period: period.Value,
                textFormatter: textFormatter,
                batchFormatter: batchFormatter,
                httpClient: httpClient);

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
