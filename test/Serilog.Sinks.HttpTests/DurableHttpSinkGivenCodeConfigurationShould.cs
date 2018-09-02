using System;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.IO;

namespace Serilog
{
    public class DurableHttpSinkGivenCodeConfigurationShould : SinkFixture
    {
        public DurableHttpSinkGivenCodeConfigurationShould()
        {
            Files.DeleteBufferFiles();

            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .DurableHttp(
                    requestUri: "some/route",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalRenderedTextFormatter(),
                    batchFormatter: new DefaultBatchFormatter(),
                    httpClient: new HttpClientMock())
                .CreateLogger();
        }

        protected override Logger Logger { get; }

        
    }
}
