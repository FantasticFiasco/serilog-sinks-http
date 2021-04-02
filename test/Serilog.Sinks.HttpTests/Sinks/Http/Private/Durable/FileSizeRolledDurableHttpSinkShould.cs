using System;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Durable
{
    public class FileSizeRolledDurableHttpSinkShould
    {
        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void ReturnSinkGivenValidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "Buffer",
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
            got.ShouldNotThrow();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "Buffer",
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

            using (new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "Buffer",
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                batchPostingLimit: 1,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromMilliseconds(1),         // 1 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: httpClient))
            {
                // Act
                await Task.Delay(TimeSpan.FromMilliseconds(10));    // Sleep 10x the period

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }
    }
}
