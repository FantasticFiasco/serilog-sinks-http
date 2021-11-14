using System;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support.Fixtures;
using Xunit;

namespace Serilog
{
    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould : SinkFixture, IClassFixture<WebServerFixture>
    {
        private readonly WebServerFixture webServerFixture;

        public DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould(WebServerFixture webServerFixture)
        {
            this.webServerFixture = webServerFixture;

            var configuration = new ConfigurationBuilder().Build();

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
                    httpClient: new JsonHttpClient(webServerFixture.CreateClient()),
                    configuration: configuration)
                .CreateLogger();

            Configuration = configuration;
        }

        protected override Logger Logger { get; }

        protected override IConfiguration Configuration { get; }
    }
}
