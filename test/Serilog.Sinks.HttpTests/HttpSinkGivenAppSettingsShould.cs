using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Support.Fixtures;
using Xunit;

namespace Serilog
{
    public class HttpSinkGivenAppSettingsShould : SinkFixture, IClassFixture<WebServerFixture>
    {
        public HttpSinkGivenAppSettingsShould(WebServerFixture webServerFixture)
            : base(webServerFixture)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        protected override Logger Logger { get; }
    }
}
