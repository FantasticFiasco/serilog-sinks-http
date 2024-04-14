using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable;

public class FileSizeRolledDurableHttpSinkShould : IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture webServerFixture;

    public FileSizeRolledDurableHttpSinkShould(WebServerFixture webServerFixture)
    {
        this.webServerFixture = webServerFixture;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void ReturnSinkGivenValidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
    {
        // Arrange
        var testId = $"ReturnSinkGivenValidBufferFileSizeLimitBytes_{Guid.NewGuid()}";

        Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
            bufferFileShared: false,
            retainedBufferFileCountLimit: 31,
            logEventLimitBytes: null,
            logEventsInBatchLimit: 1000,
            batchSizeLimitBytes: null,
            period: TimeSpan.FromSeconds(2),
            flushOnClose: true,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act & Assert
        got.ShouldNotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
    {
        // Arrange
        var testId = $"ThrowExceptionGivenInvalidBufferFileSizeLimitBytes_{Guid.NewGuid()}";

        Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
            bufferFileShared: false,
            retainedBufferFileCountLimit: 31,
            logEventLimitBytes: null,
            logEventsInBatchLimit: 1000,
            batchSizeLimitBytes: null,
            period: TimeSpan.FromSeconds(2),
            flushOnClose: true,
            textFormatter: new NormalTextFormatter(),
            batchFormatter: new ArrayBatchFormatter(),
            httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

        // Act & Assert
        got.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public async Task StayIdleGivenNoLogEvents()
    {
        // Arrange
        var testId = $"StayIdleGivenNoLogEvents_{Guid.NewGuid()}";
        var period = TimeSpan.FromMilliseconds(1);

        using (new FileSizeRolledDurableHttpSink(
                   requestUri: webServerFixture.RequestUri(testId),
                   bufferBaseFileName: Path.Combine("logs", testId),
                   bufferFileSizeLimitBytes: null,
                   bufferFileShared: false,
                   retainedBufferFileCountLimit: null,
                   logEventLimitBytes: null,
                   logEventsInBatchLimit: 1000,
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

        using var sink = new FileSizeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferFileSizeLimitBytes: null,
            bufferFileShared: false,
            retainedBufferFileCountLimit: null,
            logEventLimitBytes: 1, // Is lower than emitted log event
            logEventsInBatchLimit: 1000,
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

        var sink = new FileSizeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferFileSizeLimitBytes: null,
            bufferFileShared: false,
            retainedBufferFileCountLimit: null,
            logEventLimitBytes: null,
            logEventsInBatchLimit: 1000,
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

        var sink = new FileSizeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferFileSizeLimitBytes: null,
            bufferFileShared: false,
            retainedBufferFileCountLimit: null,
            logEventLimitBytes: null,
            logEventsInBatchLimit: 1000,
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
