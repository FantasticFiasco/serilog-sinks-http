using Microsoft.Extensions.Configuration;
using Serilog.LogServer;

namespace Serilog
{
    public class SettingsFileDurableHttpTest : SinkFixture
    {
        public SettingsFileDurableHttpTest()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_durable_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            HttpClient = TestServerHttpClient.Instance;
            HttpClient.Client = Server.CreateClient();
        }
    }
}
