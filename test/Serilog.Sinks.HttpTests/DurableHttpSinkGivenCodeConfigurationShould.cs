using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using IOFile = System.IO.File;

namespace Serilog
{
    public class DurableHttpSinkGivenCodeConfigurationShould : SinkFixture
    {
        public DurableHttpSinkGivenCodeConfigurationShould()
        {
            ClearBufferFiles();

            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .DurableHttp(
                    requestUri: "some/route",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
                    textFormatter: new NormalTextFormatter(),
                    batchFormatter: new DefaultBatchFormatter(),
                    httpClient: new HttpClientMock())
                .CreateLogger();
        }

        protected override Logger Logger { get; }

        private static void ClearBufferFiles()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Buffer*")
                .ToArray();

            foreach (var file in files)
            {
                IOFile.Delete(file);
            }
        }
    }
}
