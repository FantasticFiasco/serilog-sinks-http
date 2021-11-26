using System;
using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.Durable;
using Serilog.Sinks.Http.Private.IO;

namespace Serilog.Support.Reflection
{
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
            var rollingFileSink = sink.GetSink();
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
