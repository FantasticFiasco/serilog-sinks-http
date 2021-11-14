using System;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class TimeRolledDurableHttpSinkShould
    {
        public TimeRolledDurableHttpSinkShould()
        {
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
                requestUri: "https://www.mylogs.com",
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
                httpClient: new HttpClientMock());

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
                requestUri: "https://www.mylogs.com",
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
                httpClient: new HttpClientMock());

            // Act & Assert
            got.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var period = TimeSpan.FromMilliseconds(1);

            using (new TimeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
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
                httpClient: httpClient))
            {
                // Act
                await Task.Delay(10_000 * period);

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task RespectLogEventLimitBytes()
        {
            // Arrange
            var period = TimeSpan.FromMilliseconds(1);

            using var sink = new TimeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
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
                httpClient: httpClient);

            // Act
            sink.Emit(Some.InformationEvent());

            await Task.Delay(10_000 * period);

            // Assert
            httpClient.BatchCount.ShouldBe(0);
            httpClient.LogEvents.ShouldBeEmpty();
        }
    }
}
