using Microsoft.Extensions.Configuration;
using Serilog.Core;

namespace Serilog
{
    public class HttpSinkGivenAppSettingsShould : SinkFixture
    {
        public HttpSinkGivenAppSettingsShould()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Configuration = configuration;
        }

        protected override Logger Logger { get; }

        protected override IConfiguration Configuration { get; }
    }
}
