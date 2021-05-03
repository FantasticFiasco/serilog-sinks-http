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
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
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
                batchPostingLimit: 1000,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromSeconds(2),
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient:new HttpClientMock());

            // Act & Assert
            got.ShouldNotThrow();
        }

        [Theory]
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
                batchPostingLimit: 1000,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromSeconds(2),
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new HttpClientMock());

            // Act & Assert
            got.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            // 1 ms period
            var period = TimeSpan.FromMilliseconds(1);

            using (new TimeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "SomeBuffer",
                bufferRollingInterval: BufferRollingInterval.Day,
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                batchPostingLimit: 1,
                batchSizeLimitBytes: ByteSize.MB,
                period: period,
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: httpClient))
            {
                // Act
                await Task.Delay(10 * period);    // Sleep 10x the period

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }
    }
}
