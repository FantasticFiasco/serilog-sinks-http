using System;
using System.IO;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable;

public class TimeRolledDurableHttpSinkShould : IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture webServerFixture;

    public TimeRolledDurableHttpSinkShould(WebServerFixture webServerFixture)
    {
        this.webServerFixture = webServerFixture;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1L)]
    [InlineData(10L)]
    [InlineData(100L)]
    [InlineData(long.MaxValue)]
    public void ReturnSinkGivenValidBufferFileSizeLimitBytes(long? bufferFileSizeLimitBytes)
    {
        // Arrange
        var testId = $"ReturnSinkGivenValidBufferFileSizeLimitBytes_{Guid.NewGuid()}";

        Func<TimeRolledDurableHttpSink> got = () => new TimeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferRollingInterval: BufferRollingInterval.Day,
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
    [InlineData(0L)]
    [InlineData(-1L)]
    [InlineData(-10L)]
    [InlineData(-100L)]
    [InlineData(long.MinValue)]
    public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(long? bufferFileSizeLimitBytes)
    {
        // Arrange
        var testId = $"ThrowExceptionGivenInvalidBufferFileSizeLimitBytes_{Guid.NewGuid()}";

        Func<TimeRolledDurableHttpSink> got = () => new TimeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferRollingInterval: BufferRollingInterval.Day,
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

        using (new TimeRolledDurableHttpSink(
                   requestUri: webServerFixture.RequestUri(testId),
                   bufferBaseFileName: Path.Combine("logs", testId),
                   bufferRollingInterval: BufferRollingInterval.Day,
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

        using var sink = new TimeRolledDurableHttpSink(
            requestUri: webServerFixture.RequestUri(testId),
            bufferBaseFileName: Path.Combine("logs", testId),
            bufferRollingInterval: BufferRollingInterval.Day,
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
}
