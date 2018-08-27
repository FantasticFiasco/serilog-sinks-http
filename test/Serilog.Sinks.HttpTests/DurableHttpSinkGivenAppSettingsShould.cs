using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using IOFile = System.IO.File;

namespace Serilog
{
    public class DurableHttpSinkGivenAppSettingsShould : SinkFixture
    {
        public DurableHttpSinkGivenAppSettingsShould()
        {
            ClearBufferFiles();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
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
