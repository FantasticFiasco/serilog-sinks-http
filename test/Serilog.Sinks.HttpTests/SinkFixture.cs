using System;
using System.Linq;
using Polly;
using Serilog.Core;
using Serilog.Events;
using Serilog.LogServer;
using Serilog.Sinks.Http.LogServer;
using Serilog.Support;
using Shouldly;
using Xunit;
using Xunit.Sdk;

namespace Serilog
{
    public abstract class SinkFixture : TestServerFixture
    {
        private readonly Policy retryPolicy;

        protected SinkFixture()
        {
            retryPolicy = Policy
                .Handle<XunitException>()
                .WaitAndRetry(10, retryCount => TimeSpan.FromSeconds(1));
        }

        protected Logger Logger { get; set; }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void Level(LogEventLevel level)
        {
            // Act
            Logger.Write(level, "Some message");

            // Assert
            ExpectReceivedEvents(1);
        }

        [Theory]
        [InlineData(1)]         // 1 batch
        [InlineData(10)]        // 1 batch
        [InlineData(100)]       // ~1 batch
        [InlineData(1000)]      // ~10 batches
        [InlineData(10000)]     // ~100 batches
        public void Batches(int numberOfEvents)
        {
            // Act
            for (int i = 0; i < numberOfEvents; i++)
            {
                Logger.Information("Some message");
            }

            // Assert
            ExpectReceivedEvents(numberOfEvents);
        }

        [Fact]
        public void Payload()
        {
            // Arrange
            var expected = Some.LogEvent("Hello, {Name}!", "Alice");

            // Act
            Logger.Write(expected);

            // Assert
            var @event = ExpectReceivedEvents(1).Single();

            @event.Timestamp.ShouldBe(expected.Timestamp.DateTime);
            @event.Level.ShouldBe(expected.Level.ToString());
            @event.MessageTemplate.ShouldBe(expected.MessageTemplate.Text);
            @event.Properties["Name"].ShouldBe(expected.Properties["Name"].ToString().Trim('"'));
            @event.RenderedMessage.ShouldBe("Hello, \"Alice\"!");
            @event.Exception.ShouldBeNull();
        }

        [Fact]
        public void Exception()
        {
            // Arrange
            var expected = Some.LogEvent(LogEventLevel.Error, new Exception("Some exception"), "Some error message");

            // Act
            Logger.Write(expected);

            // Assert
            var @event = ExpectReceivedEvents(1).Single();

            @event.Timestamp.ShouldBe(expected.Timestamp.DateTime);
            @event.Level.ShouldBe(expected.Level.ToString());
            @event.MessageTemplate.ShouldBe(expected.MessageTemplate.Text);
            @event.RenderedMessage.ShouldBe("Some error message");
            @event.Exception.ShouldBe(expected.Exception.ToString());
        }

        [Fact]
        public void DropNastyException()
        {
            // Arrange
            var nastyException = Some.LogEvent(LogEventLevel.Error, new NastyException(), "Some error message");
            var expected = Some.LogEvent("Some message");

            // Act
            Logger.Write(nastyException);
            Logger.Write(expected);

            // Assert
            var @event = ExpectReceivedEvents(1).Single();

            @event.Timestamp.ShouldBe(expected.Timestamp.DateTime);
            @event.Level.ShouldBe(expected.Level.ToString());
            @event.MessageTemplate.ShouldBe(expected.MessageTemplate.Text);
            @event.Exception.ShouldBeNull();
        }

        [Fact]
        public void NetworkFailure()
        {
            // Arrange
            NetworkService.IsSimulatingNetworkFailure = true;

            // Act
            Logger.Write(LogEventLevel.Information, "Some message");

            // Assert
            ExpectReceivedEvents(1);
            TestServerHttpClient.Instance.NumberOfPosts.ShouldBeGreaterThan(1);
        }

        public override void Dispose()
        {
            base.Dispose();

            Logger?.Dispose();
            Logger = null;

            TestServerHttpClient.Instance?.Dispose();
            TestServerHttpClient.Instance = null;
        }

        private Event[] ExpectReceivedEvents(int expectedEventCount)
        {
            return retryPolicy.Execute(
                () =>
                {
                    var actual = EventService.Get()
                        .ToArray();

                    if (actual.Length > expectedEventCount)
                    {
                        throw new Exception($"Expected only {expectedEventCount} event(s) but got {actual.Length}");
                    }

                    if (actual.Length != expectedEventCount)
                    {
                        throw new XunitException($"Expected {expectedEventCount} event(s) but got {actual.Length}");
                    }

                    return actual;
                });
        }
    }
}
