using System;
using Serilog.Core;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support.Fixtures;
using Xunit;

namespace Serilog
{
    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould
        : SinkFixture, IClassFixture<WebServerFixture>
    {
        public DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould(WebServerFixture webServerFixture)
            : base(webServerFixture)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .DurableHttpUsingFileSizeRolledBuffers(
                    requestUri: webServerFixture.RequestUri(),
                    bufferBaseFileName: "SomeBuffer",
                    logEventsInBatchLimit: 100,
                    batchSizeLimitBytes: ByteSize.MB,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new ArrayBatchFormatter(),
                    httpClient: new JsonHttpClient(webServerFixture.CreateClient()))
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
