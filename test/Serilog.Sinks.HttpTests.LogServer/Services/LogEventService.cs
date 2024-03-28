using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services;

public class LogEventService
{
    private readonly BlockingCollection<BatchByTest> batchesByTest;

    public LogEventService()
    {
        batchesByTest = new BlockingCollection<BatchByTest>();
    }

    public void AddBatch(string testId, IEnumerable<LogEvent> logEvents)
    {
        foreach (var logEvent in logEvents)
        {
            Console.WriteLine($"Received {testId} {logEvent.RenderedMessage}");
        }

        batchesByTest.Add(new BatchByTest(testId, logEvents.ToArray()));
    }

    public LogEvent[][] GetAllBatches(string testId)
    {
        return batchesByTest
            .Where(batchByTest => batchByTest.TestId == testId)
            .Select(batchByTest => batchByTest.LogEvents)
            .ToArray();
    }

    public LogEvent[] GetAllEvents(string testId)
    {
        return GetAllBatches(testId)
            .SelectMany(logEvents => logEvents)
            .ToArray();
    }

    private record BatchByTest(string TestId, LogEvent[] LogEvents);
}
