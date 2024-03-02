using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.NonDurable;

public class HttpSinkShould : IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture webServerFixture;

    public HttpSinkShould(WebServerFixture webServerFixture)
    {
        this.webServerFixture = webServerFixture;
    }

    [Fact]
    public async Task StayIdleGivenNoLogEvents()
    {
        // Arrange
        var testId = $"StayIdleGivenNoLogEvents_{Guid.NewGuid()}";
        var period = TimeSpan.FromMilliseconds(1);

        using (new HttpSink(
                   requestUri: webServerFixture.RequestUri(testId),
                   queueLimitBytes: null,
                   logEventLimitBytes: null,
                   logEventsInBatchLimit: null,
                   batchSizeLimitBytes: null,
                   period: period,
                   flushOnClose: true,
                   textFormatter: new NormalTextFormatter(),
                   batchFormatter: new ArrayBatchFormatter(),
                   httpClient: new JsonHttpClient(webServerFixture.CreateClient())))
        {
            // Act
            await Task.Delay(10_000 * period);

            // Assert
            webServerFixture.GetAllBatches(testId).ShouldBeEmpty();
            webServerFixture.GetAllEvents(testId).ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task RespectLogEventLimitBytes()
    {
        // Arrange
        var testId = $"RespectLogEventLimitBytes_{Guid.NewGuid()}";
        var period = TimeSpan.FromMilliseconds(1);

        using var sink = new HttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            queueLimitBytes: null,
            logEventLimitBytes: 1, // Is lower than emitted log event
            logEventsInBatchLimit: null,
            batchSizeLimitBytes: null,
            period: period,
            flushOnClose: true,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act
        sink.Emit(Some.InformationEvent());

        await Task.Delay(10_000 * period);

        // Assert
        webServerFixture.GetAllBatches(testId).ShouldBeEmpty();
        webServerFixture.GetAllEvents(testId).ShouldBeEmpty();
    }

    [Fact]
    public async Task RespectQueueLimitBytes()
    {
        // Arrange
        var testId = $"RespectQueueLimitBytes_{Guid.NewGuid()}";

        // Create 10 log events
        var logEvents = Enumerable
            .Range(1, 10)
            .Select(number => Some.LogEvent("Event {number}", number))
            .ToArray();

        var period = TimeSpan.FromMilliseconds(10);

        using var sink = new HttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            queueLimitBytes: 134, // Queue only holds the first event, which allocates 134 bytes
            logEventLimitBytes: null,
            logEventsInBatchLimit: null,
            batchSizeLimitBytes: null,
            period: period,
            flushOnClose: true,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act
        foreach (var logEvent in logEvents)
        {
            sink.Emit(logEvent);
        }

        await Task.Delay(10_000 * period);

        // Assert
        // At least the first log events should be sent
        webServerFixture.GetAllEvents(testId).Length.ShouldBeGreaterThan(0);

        // Some log events will have been dropped
        webServerFixture.GetAllEvents(testId).Length.ShouldBeLessThan(logEvents.Length);
    }

    [Fact]
    public void SendStoredLogEventsGivenFlushOnClose()
    {
        // Arrange
        var testId = $"SendStoredLogEventsGivenFlushOnClose_{Guid.NewGuid()}";

        // Create 10 log events
        var logEvents = Enumerable
            .Range(1, 10)
            .Select(number => Some.LogEvent("Event {number}", number))
            .ToArray();

        var period = TimeSpan.FromSeconds(5);
        var flushOnClose = true;

        var sink = new HttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            queueLimitBytes: null,
            logEventLimitBytes: null,
            logEventsInBatchLimit: null,
            batchSizeLimitBytes: null,
            period: period,
            flushOnClose: flushOnClose,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act
        foreach (var logEvent in logEvents)
        {
            sink.Emit(logEvent);
        }

        sink.Dispose();

        // Assert
        webServerFixture.GetAllEvents(testId).Length.ShouldBe(logEvents.Length);
    }

    [Fact]
    public void IgnoreSendingStoredLogEventsGivenNoFlushOnClose()
    {
        // Arrange
        var testId = $"IgnoreSendingStoredLogEventsGivenNoFlushOnClose_{Guid.NewGuid()}";

        // Create 10 log events
        var logEvents = Enumerable
            .Range(1, 10)
            .Select(number => Some.LogEvent("Event {number}", number))
            .ToArray();

        var period = TimeSpan.FromSeconds(5);
        var flushOnClose = false;

        var sink = new HttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            queueLimitBytes: null,
            logEventLimitBytes: null,
            logEventsInBatchLimit: null,
            batchSizeLimitBytes: null,
            period: period,
            flushOnClose: flushOnClose,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act
        foreach (var logEvent in logEvents)
        {
            sink.Emit(logEvent);
        }

        sink.Dispose();

        // Assert
        webServerFixture.GetAllEvents(testId).Length.ShouldBe(0);
    }
}
