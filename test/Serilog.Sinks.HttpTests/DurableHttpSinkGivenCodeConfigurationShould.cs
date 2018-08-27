using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Support;
using Serilog.Support.TextFormatters;
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
                    textFormatter: new RenderedMessageTextFormatter(),
                    batchFormatter: new ArrayBatchFormatter(),
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
