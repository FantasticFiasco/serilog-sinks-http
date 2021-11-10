using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    [ApiController]
    [Route("logs")]
    public class LogEventController : ControllerBase
    {
        private readonly LogEventService logEventService;

        public LogEventController(LogEventService logEventService)
        {
            this.logEventService = logEventService;
        }

        [HttpPost]
        public void Post(LogEventDto logEvent)
        {
            logEventService.Add(logEvent.ToLogEvent());
        }

        [HttpGet]
        public LogEventDto[] Get()
        {
            return logEventService
                .GetAll()
                .Select(logEvent => new LogEventDto(
                    logEvent.Timestamp,
                    logEvent.Level,
                    logEvent.MessageTemplate,
                    logEvent.RenderedMessage,
                    logEvent.Properties))
                .ToArray();
        }
    }
}
