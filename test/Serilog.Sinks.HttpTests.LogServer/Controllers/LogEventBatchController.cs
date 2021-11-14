using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    [ApiController]
    [Route("batches")]
    public class LogEventBatchController
    {
        private readonly LogEventService logEventService;

        public LogEventBatchController(LogEventService logEventService)
        {
            this.logEventService = logEventService;
        }

        [HttpPost]
        public void Post(LogEventDto[] batch)
        {
            var logEvents = batch.Select(logEvent => logEvent.ToLogEvent());
            logEventService.AddBatch(logEvents);
        }
    }
}
