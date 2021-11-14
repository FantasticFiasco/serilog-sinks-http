using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers;

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
    [Route("default-batch")]
    public void Post(DefaultBatchDto batch)
    {
        Post(batch.Events);
    }

    [HttpPost]
    [Route("array-batch")]
    public void Post(LogEventDto[] batch)
    {
        var logEvents = batch.Select(logEvent => logEvent.ToLogEvent());
        logEventService.AddBatch(logEvents);
    }

    [HttpGet]
    public LogEventDto[] Get()
    {
        return logEventService
            .GetAll()
            .Select(LogEventDto.From)
            .ToArray();
    }
}
