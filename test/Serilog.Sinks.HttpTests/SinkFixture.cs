using System;
using Serilog.Core;
using Serilog.Events;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog
{
    // TODO: Add test that congiguration is passed to HTTP client

    public abstract class SinkFixture : IDisposable
    {
        protected SinkFixture(WebServerFixture webServerFixture)
        {
            WebServerFixture = webServerFixture ?? throw new ArgumentNullException(nameof(webServerFixture));

            BufferFiles.Delete();
        }

        protected abstract Logger Logger { get; }

        protected WebServerFixture WebServerFixture { get; private set; }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void WriteLogEvent(LogEventLevel level)
        {
            // Act
            Logger.Write(level, "Some message");

            // Assert
            WebServerFixture.GetAllEvents().Length.ShouldBe(1);
        }

        [Theory]
        [InlineData(1)]         // 1 batch assuming batch size is 100
        [InlineData(10)]        // 1 batch assuming batch size is 100
        [InlineData(100)]       // ~1 batch assuming batch size is 100
        [InlineData(1_000)]      // ~10 batches assuming batch size is 100
        [InlineData(10_000)]     // ~100 batches assuming batch size is 100
        public void WriteBatches(int numberOfEvents)
        {
            // Act
            for (int i = 0; i < numberOfEvents; i++)
            {
                Logger.Information("Some message");
            }

            // Assert
            WebServerFixture.GetAllEvents().Length.ShouldBe(numberOfEvents);
        }

        [Fact]
        public void OvercomeNetworkFailure()
        {
            // TODO: Fix text
            throw new Exception("Fix me!");

            //// Arrange
            //HttpClientMock.Instance.SimulateNetworkFailure();

            //// Act
            //Logger.Write(LogEventLevel.Information, "Some message");

            //// Assert
            //await HttpClientMock.Instance.WaitAsync(1);

            //HttpClientMock.Instance.BatchCount.ShouldBe(1);
            //HttpClientMock.Instance.LogEvents.Length.ShouldBe(1);
        }

        public void Dispose()
        {
            Logger.Dispose();
        }
    }
}
