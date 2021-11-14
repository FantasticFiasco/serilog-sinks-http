using Microsoft.AspNetCore.Mvc.Testing;
using Serilog.Sinks.HttpTests.LogServer;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Support.Fixtures
{
    public class WebServerFixture : WebApplicationFactory<Startup>
    {
        public string Route(string path)
        {
            return Server.BaseAddress + path;
        }

        public LogEvent[][] GetAllBatches()
        {
            return GetLogEventService().GetAllBatches();
        }

        public LogEvent[] GetAllEvents()
        {
            return GetLogEventService().GetAllEvents();
        }

        private LogEventService GetLogEventService()
        {
            return (LogEventService)Services.GetService(typeof(LogEventService));
        }
    }
}
