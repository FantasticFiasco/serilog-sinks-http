using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers;

[ApiController]
[Route("log-events")]
public class LogEventController : ControllerBase
{
    private readonly LogEventService logEventService;

    public LogEventController(LogEventService logEventService)
    {
        this.logEventService = logEventService;
    }

    [HttpGet]
    public LogEventDto[] Get()
    {
        return logEventService
            .GetAllEvents()
            .Select(LogEventDto.From)
            .ToArray();
    }
}
