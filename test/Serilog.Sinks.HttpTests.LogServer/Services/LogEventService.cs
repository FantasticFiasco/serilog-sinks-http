using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services;

public class LogEventService
{
    private readonly BlockingCollection<LogEvent[]> logEventBatches;

    public LogEventService()
    {
        logEventBatches = new BlockingCollection<LogEvent[]>();
    }

    public void AddBatch(IEnumerable<LogEvent> logEvents)
    {
        logEventBatches.Add(logEvents.ToArray());
    }

    public LogEvent[][] GetAllBatches()
    {
        return logEventBatches.ToArray();
    }

    public LogEvent[] GetAllEvents()
    {
        return GetAllBatches().SelectMany(batch => batch).ToArray();
    }
}
