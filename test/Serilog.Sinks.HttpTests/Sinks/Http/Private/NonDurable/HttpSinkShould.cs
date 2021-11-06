using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class HttpSinkShould
    {
        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using (new HttpSink(
                requestUri: "https://www.mylogs.com",
                logEventLimitBytes: null,
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: null,
                period: TimeSpan.FromMilliseconds(1), // 1 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: httpClient))
            {
                // Act
                await Task.Delay(TimeSpan.FromSeconds(10)); // Sleep 10000x the period

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task RespectLogEventLimitBytes()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using var sink = new HttpSink(
                requestUri: "https://www.mylogs.com",
                logEventLimitBytes: 1, // Is lower than emitted log event
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: null,
                period: TimeSpan.FromMilliseconds(1), // 1 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: httpClient);

            // Act
            sink.Emit(Some.InformationEvent());

            await Task.Delay(TimeSpan.FromSeconds(10)); // Sleep 10000x the period

            // Assert
            httpClient.BatchCount.ShouldBe(0);
            httpClient.LogEvents.ShouldBeEmpty();
        }

        [Fact]
        public async Task RespectQueueLimitBytes()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            // Create 10 log events
            var logEvents = Enumerable
                .Range(1, 10)
                .Select(number => Some.LogEvent("Event {number}", number))
                .ToArray();

            using var sink = new HttpSink(
                requestUri: "https://www.mylogs.com",
                logEventLimitBytes: null,
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: 134, // Queue only holds the first event, which allocates 134 bytes
                period: TimeSpan.FromMilliseconds(10), // 10 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: httpClient);

            // Act
            foreach (var logEvent in logEvents)
            {
                sink.Emit(logEvent);
            }

            await Task.Delay(TimeSpan.FromSeconds(10)); // Sleep 1000x the period

            // Assert
            httpClient.LogEvents.Length.ShouldBeGreaterThan(0);
            httpClient.LogEvents.Length.ShouldBeLessThan(logEvents.Length); // Some log events will have been dropped
        }
    }
}
