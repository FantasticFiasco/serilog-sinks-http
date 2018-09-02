using System;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;

namespace Serilog
{
    public class HttpSinkGivenCodeConfigurationShould : SinkFixture
    {
        public HttpSinkGivenCodeConfigurationShould() =>
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .Http(
                    requestUri: "some/route",
                    batchPostingLimit: 100,
                    queueLimit: 10000,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new DefaultBatchFormatter(),
                    httpClient: new HttpClientMock())
                .CreateLogger();

        protected override Logger Logger { get; }
    }
}
