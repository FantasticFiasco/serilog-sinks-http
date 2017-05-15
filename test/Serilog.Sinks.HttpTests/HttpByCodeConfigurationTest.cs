using System;
using Serilog.LogServer;

namespace Serilog
{
    public class HttpByCodeConfigurationTest : SinkFixture
    {
        public HttpByCodeConfigurationTest()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .Http(
                    requestUri: "api/events",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
                    httpClient: new TestServerHttpClient())
                .CreateLogger();

            TestServerHttpClient.Instance.Client = Server.CreateClient();
        }
    }
}
