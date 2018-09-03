using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Serilog
{
    public class DurableHttpSinkGivenAppSettingsShould : SinkFixture
    {
        public DurableHttpSinkGivenAppSettingsShould()
        {
            DeleteBufferFiles();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
