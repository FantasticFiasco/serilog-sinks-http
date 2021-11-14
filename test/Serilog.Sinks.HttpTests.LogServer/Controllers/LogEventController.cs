using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    [ApiController]
    public class LogEventController
    {
        private readonly LogEventService logEventService;

        public LogEventController(LogEventService logEventService)
        {
            this.logEventService = logEventService;
        }

        [HttpPost]
        [Route("logs/{testName}")]
        public void Post(string testName, LogEventDto[] batch)
        {
            var logEvents = batch.Select(logEvent => logEvent.ToLogEvent());
            logEventService.AddBatch(testName, logEvents);
        }
    }
}
