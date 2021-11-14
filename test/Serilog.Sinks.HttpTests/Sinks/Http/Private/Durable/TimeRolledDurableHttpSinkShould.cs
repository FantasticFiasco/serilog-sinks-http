using System;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class TimeRolledDurableHttpSinkShould : IClassFixture<WebServerFixture>
    {
        private readonly WebServerFixture webServerFixture;

        public TimeRolledDurableHttpSinkShould(WebServerFixture webServerFixture)
        {
            this.webServerFixture = webServerFixture;

            BufferFiles.Delete();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void ReturnSinkGivenValidBufferFileSizeLimitBytes(long? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<TimeRolledDurableHttpSink> got = () => new TimeRolledDurableHttpSink(
                requestUri: webServerFixture.RequestUri(),
                bufferBaseFileName: "SomeBuffer",
                bufferRollingInterval: BufferRollingInterval.Day,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: false,
                retainedBufferFileCountLimit: 31,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromSeconds(2),
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
        public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(long? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<TimeRolledDurableHttpSink> got = () => new TimeRolledDurableHttpSink(
                requestUri: webServerFixture.RequestUri(),
                bufferBaseFileName: "SomeBuffer",
                bufferRollingInterval: BufferRollingInterval.Day,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: false,
                retainedBufferFileCountLimit: 31,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromSeconds(2),
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
            var period = TimeSpan.FromMilliseconds(1);

            using (new TimeRolledDurableHttpSink(
                requestUri: webServerFixture.RequestUri(),
                bufferBaseFileName: "SomeBuffer",
                bufferRollingInterval: BufferRollingInterval.Day,
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
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

            using var sink = new TimeRolledDurableHttpSink(
                requestUri: webServerFixture.RequestUri(),
                bufferBaseFileName: "SomeBuffer",
                bufferRollingInterval: BufferRollingInterval.Day,
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                logEventLimitBytes: 1, // Is lower than emitted log event
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
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
    }
}
