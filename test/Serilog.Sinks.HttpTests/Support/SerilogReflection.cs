using System;
using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.Durable;
using Serilog.Sinks.Http.Private.IO;
using Serilog.Sinks.Http.Private.NonDurable;

namespace Serilog.Support
{
    public static class SerilogReflection
    {
        public static HttpSinkReflection GetHttpSink(Logger logger)
        {
            var sink = GetSink<HttpSink>(logger);
            return new HttpSinkReflection(sink);
        }

        public static TimeRolledDurableHttpSinkReflection GetTimeRolledDurableHttpSink(Logger logger)
        {
            var sink = GetSink<TimeRolledDurableHttpSink>(logger);
            return new TimeRolledDurableHttpSinkReflection(sink);
        }

        private static T GetSink<T>(Logger logger) where T : ILogEventSink
        {
            var sinks = logger
                .GetNonPublicInstanceField<object>("_sink")
                .GetNonPublicInstanceField<ILogEventSink[]>("_sinks");

            foreach (var sink in sinks)
            {
                if (sink is T t)
                {
                    return t;
                }
            }

            throw new Exception($"Logger does not contain a sink of type {typeof(T)}.");
        }

        private static object GetSink(Logger logger)
        {
            var sinks = logger
                .GetNonPublicInstanceField<object>("_sink")
                .GetNonPublicInstanceField<ILogEventSink[]>("_sinks");

            if (sinks.Length != 1)
            {
                throw new Exception("Logger contains more than one sink.");
            }

            return sinks[0];
        }

        public class HttpSinkReflection
        {
            private readonly HttpSink sink;

            public HttpSinkReflection(HttpSink sink)
            {
                this.sink = sink ?? throw new ArgumentNullException(nameof(sink));
            }

            public HttpSinkReflection SetRequestUri(string requestUri)
            {
                sink.SetNonPublicInstanceField("requestUri", requestUri);
                return this;
            }

            public HttpSinkReflection SetHttpClient(IHttpClient httpClient)
            {
                sink.SetNonPublicInstanceField("httpClient", httpClient);
                return this;
            }
        }

        public class TimeRolledDurableHttpSinkReflection
        {
            private readonly TimeRolledDurableHttpSink sink;

            public TimeRolledDurableHttpSinkReflection(TimeRolledDurableHttpSink sink)
            {
                this.sink = sink ?? throw new ArgumentNullException(nameof(sink));
            }

            public TimeRolledDurableHttpSinkReflection SetRequestUri(string requestUri)
            {
                sink
                    .GetNonPublicInstanceField<HttpLogShipper>("shipper")
                    .SetNonPublicInstanceField("requestUri", requestUri);

                return this;
            }

            public TimeRolledDurableHttpSinkReflection SetBufferBaseFileName(string bufferBaseFileName)
            {
                // Update shipper
                var shipper = this.sink.GetNonPublicInstanceField<HttpLogShipper>("shipper");
                var timeRolledBufferFiles = new TimeRolledBufferFiles(new DirectoryService(), bufferBaseFileName);
                shipper.SetNonPublicInstanceField("bufferFiles", timeRolledBufferFiles);

                // Update file sink
                var sink = this.sink.GetNonPublicInstanceField<Logger>("sink");
                var rollingFileSink = GetSink(sink);
                var roller = rollingFileSink.GetNonPublicInstanceField<object>("_roller");

                var bufferRollingInterval = roller.GetNonPublicInstanceField<RollingInterval>("_interval");
                var bufferFileSizeLimitBytes = rollingFileSink.GetNonPublicInstanceField<long?>("_fileSizeLimitBytes");
                var bufferFileShared = rollingFileSink.GetNonPublicInstanceField<bool>("_shared");
                var retainedBufferFileCountLimit = rollingFileSink.GetNonPublicInstanceField<int?>("_retainedFileCountLimit");
                var textFormatter = rollingFileSink.GetNonPublicInstanceField<ITextFormatter>("_textFormatter");

                rollingFileSink = this.sink.InvokeNonPublicStaticMethod<ILogEventSink>(
                    "CreateFileSink",
                    bufferBaseFileName,
                    bufferRollingInterval,
                    bufferFileSizeLimitBytes,
                    bufferFileShared,
                    retainedBufferFileCountLimit,
                    textFormatter);

                this.sink.SetNonPublicInstanceField("sink", rollingFileSink);

                return this;
            }

            public TimeRolledDurableHttpSinkReflection SetHttpClient(IHttpClient httpClient)
            {
                sink
                    .GetNonPublicInstanceField<HttpLogShipper>("shipper")
                    .SetNonPublicInstanceField("httpClient", httpClient);

                return this;
            }
        }
    }
}
