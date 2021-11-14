using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services;

public class LogEventService
{
    private readonly BlockingCollection<BatchByTest> batchesByTest;

    public LogEventService()
    {
        batchesByTest = new BlockingCollection<BatchByTest>();
    }

    public void AddBatch(string testName, IEnumerable<LogEvent> logEvents)
    {
        batchesByTest.Add(new BatchByTest(testName, logEvents.ToArray()));
    }

    public LogEvent[][] GetAllBatches(string testName)
    {
        return batchesByTest
            .Where(batchByTest => batchByTest.testName == testName)
            .Select(batchByTest => batchByTest.logEvents)
            .ToArray();
    }

    public LogEvent[] GetAllEvents(string testName)
    {
        return GetAllBatches(testName)
            .SelectMany(logEvents => logEvents)
            .ToArray();
    }

    private record BatchByTest(string testName, LogEvent[] logEvents);
}
