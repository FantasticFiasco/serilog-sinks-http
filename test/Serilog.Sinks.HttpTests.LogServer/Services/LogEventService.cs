using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services;

public class LogEventService
{
    private readonly BlockingCollection<LogEvent> logEvents;

    public LogEventService()
    {
        logEvents = new BlockingCollection<LogEvent>();
    }

    public void Add(LogEvent logEvent)
    {
        logEvents.Add(logEvent);
    }

    public LogEvent[] GetAll()
    {
        return logEvents.ToArray();
    }
}
