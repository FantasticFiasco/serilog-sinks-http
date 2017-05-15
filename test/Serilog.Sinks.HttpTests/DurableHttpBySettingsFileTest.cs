using Microsoft.Extensions.Configuration;
using Serilog.LogServer;

namespace Serilog
{
    public class DurableHttpBySettingsFileTest : SinkFixture
    {
        public DurableHttpBySettingsFileTest()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            TestServerHttpClient.Instance.Client = Server.CreateClient();
        }
    }
}
