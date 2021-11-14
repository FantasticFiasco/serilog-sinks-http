using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Support.Fixtures;
using Xunit;

namespace Serilog
{
    public class DurableHttpSinkUsingTimeRolledBuffersGivenAppSettingsShould
        : SinkFixture, IClassFixture<WebServerFixture>
    {
        public DurableHttpSinkUsingTimeRolledBuffersGivenAppSettingsShould(WebServerFixture webServerFixture)
            : base(webServerFixture)
        {
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
