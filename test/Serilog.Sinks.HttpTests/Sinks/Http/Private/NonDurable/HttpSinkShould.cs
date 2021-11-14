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

namespace Serilog.Sinks.Http.Private.NonDurable
{
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
            var period = TimeSpan.FromMilliseconds(1);

            using (new HttpSink(
                requestUri: webServerFixture.RequestUri(),
                logEventLimitBytes: null,
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: null,
                period: period,
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient())))
            {
                // Act
                await Task.Delay(10_000 * period);

                // Assert
                webServerFixture.GetAllBatches().ShouldBeEmpty();
                webServerFixture.GetAllEvents().ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task RespectLogEventLimitBytes()
        {
            // Arrange
            var period = TimeSpan.FromMilliseconds(1);

            using var sink = new HttpSink(
                requestUri: webServerFixture.RequestUri(),
                logEventLimitBytes: 1, // Is lower than emitted log event
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: null,
                period: period,
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

            // Act
            sink.Emit(Some.InformationEvent());

            await Task.Delay(10_000 * period);

            // Assert
            webServerFixture.GetAllBatches().ShouldBeEmpty();
            webServerFixture.GetAllEvents().ShouldBeEmpty();
        }

        [Fact]
        public async Task RespectQueueLimitBytes()
        {
            // Arrange
            // Create 10 log events
            var logEvents = Enumerable
                .Range(1, 10)
                .Select(number => Some.LogEvent("Event {number}", number))
                .ToArray();

            var period = TimeSpan.FromMilliseconds(10);

            using var sink = new HttpSink(
                requestUri: webServerFixture.RequestUri(),
                logEventLimitBytes: null,
                logEventsInBatchLimit: null,
                batchSizeLimitBytes: null,
                queueLimitBytes: 134, // Queue only holds the first event, which allocates 134 bytes
                period: period, 
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient()));

            // Act
            foreach (var logEvent in logEvents)
            {
                sink.Emit(logEvent);
            }

            await Task.Delay(1_000 * period);

            // Assert
            webServerFixture.GetAllEvents().Length.ShouldBeGreaterThan(0);
            webServerFixture.GetAllEvents().Length.ShouldBeLessThan(logEvents.Length); // Some log events will have been dropped
        }
    }
}
