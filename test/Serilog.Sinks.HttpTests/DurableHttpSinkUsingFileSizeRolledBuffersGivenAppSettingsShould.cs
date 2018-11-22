using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Serilog
{
    public class DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould : SinkFixture
    {
        public DurableHttpSinkUsingFileSizeRolledBuffersGivenAppSettingsShould()
        {
            DeleteBufferFiles();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http_using_file_size_rolled_buffers.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
