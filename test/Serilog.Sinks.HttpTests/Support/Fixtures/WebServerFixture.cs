using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Serilog.Sinks.HttpTests.LogServer;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Support.Fixtures
{
    public class WebServerFixture : WebApplicationFactory<Startup>
    {
        public string RequestUri([CallerMemberName] string testName = "")
        {
            return Server.BaseAddress + "logs/" + WebUtility.UrlEncode(testName);
        }

        public LogEvent[][] GetAllBatches([CallerMemberName] string testName = "")
        {
            return GetLogEventService().GetAllBatches(testName);
        }

        public LogEvent[] GetAllEvents([CallerMemberName] string testName = "")
        {
            return GetLogEventService().GetAllEvents(testName);
        }

        private LogEventService GetLogEventService()
        {
            return (LogEventService)Services.GetService(typeof(LogEventService));
        }
    }
}
