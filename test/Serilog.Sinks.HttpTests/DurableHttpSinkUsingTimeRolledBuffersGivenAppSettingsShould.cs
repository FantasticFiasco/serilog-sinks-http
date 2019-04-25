using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Serilog
{
    public class DurableHttpSinkUsingTimeRolledBuffersGivenAppSettingsShould : SinkFixture
    {
        public DurableHttpSinkUsingTimeRolledBuffersGivenAppSettingsShould()
        {
            DeleteBufferFiles();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http_using_time_rolled_buffers.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
