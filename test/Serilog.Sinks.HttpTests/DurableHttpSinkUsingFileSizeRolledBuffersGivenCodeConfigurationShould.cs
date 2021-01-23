using System;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;

namespace Serilog
{
    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould : SinkFixture
    {
        public DurableHttpSinkUsingFileSizeRolledBuffersGivenCodeConfigurationShould()
        {
            DeleteBufferFiles();

            var configuration = new ConfigurationBuilder().Build();

            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .DurableHttpUsingFileSizeRolledBuffers(
                    requestUri: "some/route",
                    batchPostingLimit: 100,
                    batchSizeLimitBytes: 1048576,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new DefaultBatchFormatter(),
                    httpClient: new HttpClientMock(),
                    configuration: configuration)
                .CreateLogger();

            Configuration = configuration;
        }

        protected override Logger Logger { get; }

        protected override IConfiguration Configuration { get; }
    }
}
