using Microsoft.Extensions.Configuration;
using Serilog.LogServer;

namespace Serilog
{
    public class SettingsFileHttpTest : SinkFixture
    {
        public SettingsFileHttpTest()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            HttpClient = TestServerHttpClient.Instance;
            HttpClient.Client = Server.CreateClient();
        }
    }
}
