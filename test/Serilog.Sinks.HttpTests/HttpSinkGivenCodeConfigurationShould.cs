using System;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;

namespace Serilog
{
    public class HttpSinkGivenCodeConfigurationShould : SinkFixture
    {
        public HttpSinkGivenCodeConfigurationShould()
        {
            var configuration = new ConfigurationBuilder().Build();

            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .Http(
                    requestUri: "some/route",
                    batchPostingLimit: 100,
                    batchSizeLimitBytes: 1048576,
                    queueLimit: 10000,
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
