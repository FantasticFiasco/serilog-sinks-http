using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    [ApiController]
    [Route("logs")]
    public class LogEventController : ControllerBase
    {
        private readonly LogEventService _logEventService;

        public LogEventController(LogEventService logEventService)
        {
            _logEventService = logEventService;
        }

        [HttpPost]
        public void Post(string logEvent)
        {
            _logEventService.Add(logEvent);
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _logEventService.GetAll();
        }
    }
}
