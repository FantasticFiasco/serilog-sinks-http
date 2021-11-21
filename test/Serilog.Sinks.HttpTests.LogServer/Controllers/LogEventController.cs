using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    [ApiController]
    public class LogEventController : ControllerBase
    {
        private readonly HealthService healthService;
        private readonly LogEventService logEventService;

        public LogEventController(HealthService healthService, LogEventService logEventService)
        {
            this.healthService = healthService;
            this.logEventService = logEventService;
        }

        [HttpPost]
        [Route("logs/{testId}")]
        public IActionResult Post(string testId, LogEventDto[] batch)
        {
            if (!healthService.GetIsHealthy())
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            var logEvents = batch.Select(logEvent => logEvent.ToLogEvent());
            logEventService.AddBatch(testId, logEvents);
            return NoContent();
        }
    }
}
