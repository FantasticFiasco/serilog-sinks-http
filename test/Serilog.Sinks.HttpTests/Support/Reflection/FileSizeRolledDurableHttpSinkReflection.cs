using Serilog.Core;
using Serilog.Formatting;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.Private.Durable;
using Serilog.Sinks.Http.Private.IO;

namespace Serilog.Support.Reflection
{
    public class FileSizeRolledDurableHttpSinkReflection
    {
        private readonly FileSizeRolledDurableHttpSink sink;

        public FileSizeRolledDurableHttpSinkReflection(Logger logger)
        {
            sink = logger.GetSink<FileSizeRolledDurableHttpSink>();
        }

        public FileSizeRolledDurableHttpSinkReflection SetRequestUri(string requestUri)
        {
            sink
                .GetNonPublicInstanceField<HttpLogShipper>("shipper")
                .SetNonPublicInstanceField("requestUri", requestUri);

            return this;
        }

        public FileSizeRolledDurableHttpSinkReflection SetBufferBaseFileName(string bufferBaseFileName)
        {
            // Update shipper
            var shipper = this.sink.GetNonPublicInstanceField<HttpLogShipper>("shipper");
            var timeRolledBufferFiles = new FileSizeRolledBufferFiles(new DirectoryService(), bufferBaseFileName);
            shipper.SetNonPublicInstanceField("bufferFiles", timeRolledBufferFiles);

            // Update file sink
            var sink = this.sink.GetNonPublicInstanceField<Logger>("sink");
            var rollingFileSink = sink.GetSink();
            
            var bufferFileSizeLimitBytes = rollingFileSink.GetNonPublicInstanceField<long?>("_fileSizeLimitBytes");
            var bufferFileShared = rollingFileSink.GetNonPublicInstanceField<bool>("_shared");
            var retainedBufferFileCountLimit = rollingFileSink.GetNonPublicInstanceField<int?>("_retainedFileCountLimit");
            var textFormatter = rollingFileSink.GetNonPublicInstanceField<ITextFormatter>("_textFormatter");

            rollingFileSink = this.sink.InvokeNonPublicStaticMethod<ILogEventSink>(
                "CreateFileSink",
                bufferBaseFileName,
                bufferFileSizeLimitBytes,
                bufferFileShared,
                retainedBufferFileCountLimit,
                textFormatter);

            this.sink.SetNonPublicInstanceField("sink", rollingFileSink);

            return this;
        }

        public FileSizeRolledDurableHttpSinkReflection SetHttpClient(IHttpClient httpClient)
        {
            sink
                .GetNonPublicInstanceField<HttpLogShipper>("shipper")
                .SetNonPublicInstanceField("httpClient", httpClient);

            return this;
        }
    }
}
