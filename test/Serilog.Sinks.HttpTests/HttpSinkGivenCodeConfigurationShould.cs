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
    public class HttpSinkGivenCodeConfigurationShould : SinkFixture, IClassFixture<WebServerFixture>
    {
        public HttpSinkGivenCodeConfigurationShould(WebServerFixture webServerFixture)
            : base(webServerFixture)
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .Http(
                    requestUri: WebServerFixture.RequestUri(),
                    logEventsInBatchLimit: 100,
                    batchSizeLimitBytes: ByteSize.MB,
                    queueLimitBytes: ByteSize.MB,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new ArrayBatchFormatter(),
                    httpClient: new JsonHttpClient(WebServerFixture.CreateClient()))
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
