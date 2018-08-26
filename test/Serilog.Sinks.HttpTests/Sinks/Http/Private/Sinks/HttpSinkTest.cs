using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Formatting.Display;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
{
    public class HttpSinkTest
    {
        [Fact]
        public async Task NoNetworkTrafficWithoutLogEvents()
        {
            // Arrange
            var httpClient = new InMemoryHttpClient();

            // ReSharper disable once UnusedVariable
            var httpSink = new HttpSink(
                "api/events",
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                httpClient);

            // Act
            await Task.Delay(TimeSpan.FromMinutes(3));

            // Assert
            httpClient.Events.ShouldBeEmpty();
        }

        [Fact]
        public async Task RespectQueueLimit()
        {
            // Arrange
            var httpClient = new InMemoryHttpClient();

            var httpSink = new HttpSink(
                "api/events",
                1,
                1,  // Queue only holds 1 event
                TimeSpan.FromSeconds(2),
                new MessageTemplateTextFormatter("{Message}", null),
                new DefaultBatchFormatter(),
                httpClient);

            // Act
            Enumerable
                .Range(1, 10)
                .ToList()
                .ForEach(number => httpSink.Emit(Some.LogEvent("Event {number}", number)));

            await Task.Delay(TimeSpan.FromSeconds(4));

            // Assert
            httpClient.Events.Length.ShouldBe(1);
            (await httpClient.Events[0].ReadAsStringAsync()).ShouldBe("{\"events\":[Event 1]}");
        }
    }
}
