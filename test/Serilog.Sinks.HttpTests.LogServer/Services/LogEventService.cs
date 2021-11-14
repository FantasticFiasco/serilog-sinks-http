using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services;

public class LogEventService
{
    private readonly BlockingCollection<LogEvent> logEvents;
    private int numberOfBatches;

    public LogEventService()
    {
        logEvents = new BlockingCollection<LogEvent>();
    }

    public void AddBatch(IEnumerable<LogEvent> logEvents)
    {
        Interlocked.Increment(ref numberOfBatches);

        foreach (var logEvent in logEvents)
        {
            this.logEvents.Add(logEvent);
        }
    }

    public LogEvent[] GetAll()
    {
        return logEvents.ToArray();
    }
}
