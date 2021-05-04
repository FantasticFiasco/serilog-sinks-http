using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog
{
    public abstract class SinkFixture : IDisposable
    {
        protected SinkFixture()
        {
            DeleteBufferFiles();
        }

        protected abstract Logger Logger { get; }
        protected abstract IConfiguration Configuration { get; }

        [Fact]
        public void ConfigureHttpClient()
        {
            // Assert
            HttpClientMock.Instance.Configuration.ShouldBe(Configuration);
        }

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

        private static void DeleteBufferFiles()
        {
            var files = Directory.GetFiles(Directory.GetCurrentDirectory());

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                var delete = fileName.EndsWith(".bookmark")
                    || (fileName.Contains("Buffer") && fileName.EndsWith(".json"));
                
                if (delete)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
