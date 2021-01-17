using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
{
    public class HttpSinkShould
    {
        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using (new HttpSink(
                "some/route",
                1,
                TimeSpan.FromMilliseconds(1),         // 1 ms period
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                httpClient))
            {
                // Act
                await Task.Delay(TimeSpan.FromMilliseconds(10));    // Sleep 10x the period

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task RespectQueueLimit()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            // Create 10 log events
            var logEvents = Enumerable
                .Range(1, 10)
                .Select(number => Some.LogEvent("Event {number}", number))
                .ToArray();

            using var sink = new HttpSink(
                "some/route",
                1,
                1,                                   // Queue only holds 1 event
                TimeSpan.FromMilliseconds(1),        // 1 ms period
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                httpClient);
            // Act
            foreach (var logEvent in logEvents)
            {
                sink.Emit(logEvent);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));    // Sleep 10x the period

            // Assert
            httpClient.LogEvents.Length.ShouldBeLessThan(logEvents.Length);    // Some log events will have been dropped
        }
    }
}
