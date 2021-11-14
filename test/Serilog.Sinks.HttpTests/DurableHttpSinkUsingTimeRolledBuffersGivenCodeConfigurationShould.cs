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
    public class DurableHttpSinkUsingTimeRolledBuffersGivenCodeConfigurationShould
        : SinkFixture, IClassFixture<WebServerFixture>
    {
        public DurableHttpSinkUsingTimeRolledBuffersGivenCodeConfigurationShould(WebServerFixture webServerFixture)
            : base(webServerFixture)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .DurableHttpUsingTimeRolledBuffers(
                    requestUri: WebServerFixture.RequestUri(),
                    bufferBaseFileName: "SomeBuffer",
                    bufferRollingInterval: BufferRollingInterval.Hour,
                    logEventsInBatchLimit: 100,
                    batchSizeLimitBytes: ByteSize.MB,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new ArrayBatchFormatter(),
                    httpClient: new JsonHttpClient(WebServerFixture.CreateClient()))
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
