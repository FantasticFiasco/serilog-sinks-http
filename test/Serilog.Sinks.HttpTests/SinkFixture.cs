using System;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog
{
    public abstract class SinkFixture : IDisposable
    {
        protected abstract Logger Logger { get; }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public async Task WriteLogEvent(LogEventLevel level)
        {
            // Act
            Logger.Write(level, "Some message");

            // Assert
            await HttpClientMock.Instance.WaitAsync(1);
        }

        [Theory]
        [InlineData(1)]         // 1 batch assuming batch size is 100
        [InlineData(10)]        // 1 batch assuming batch size is 100
        [InlineData(100)]       // ~1 batch assuming batch size is 100
        [InlineData(1000)]      // ~10 batches assuming batch size is 100
        [InlineData(10000)]     // ~100 batches assuming batch size is 100
        public async Task WriteBatches(int numberOfEvents)
        {
            // Act
            for (int i = 0; i < numberOfEvents; i++)
            {
                Logger.Information("Some message");
            }

            // Assert
            await HttpClientMock.Instance.WaitAsync(numberOfEvents);
        }

        [Fact]
        public async Task OvercomeNetworkFailure()
        {
            // Arrange
            HttpClientMock.Instance.SimulateNetworkFailure();

            // Act
            Logger.Write(LogEventLevel.Information, "Some message");

            // Assert
            await HttpClientMock.Instance.WaitAsync(1);

            HttpClientMock.Instance.BatchCount.ShouldBe(1);
            HttpClientMock.Instance.LogEvents.Length.ShouldBe(1);
        }

        public void Dispose()
        {
            Logger.Dispose();
            HttpClientMock.Instance.Dispose();
        }

        protected static void DeleteBufferFiles()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Buffer*")
                .ToArray();

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
