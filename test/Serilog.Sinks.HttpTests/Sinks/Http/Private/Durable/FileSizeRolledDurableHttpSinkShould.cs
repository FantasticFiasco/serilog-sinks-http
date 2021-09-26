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
        public FileSizeRolledDurableHttpSinkShould()
        {
            BufferFiles.Delete();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void ReturnSinkGivenValidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "SomeBuffer",
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: false,
                retainedBufferFileCountLimit: 31,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromSeconds(2),
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
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
        public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<FileSizeRolledDurableHttpSink> got = () => new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "SomeBuffer",
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: false,
                retainedBufferFileCountLimit: 31,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromSeconds(2),
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: new HttpClientMock());

            // Act & Assert
            got.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using (new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "SomeBuffer",
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                logEventLimitBytes: null,
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromMilliseconds(1), // 1 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: httpClient))
            {
                // Act
                await Task.Delay(TimeSpan.FromSeconds(10)); // Sleep 10000x the period

                // Assert
                httpClient.BatchCount.ShouldBe(0);
                httpClient.LogEvents.ShouldBeEmpty();
            }
        }

        [Fact]
        public async Task RespectLogEventLimitBytes()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using var sink = new FileSizeRolledDurableHttpSink(
                requestUri: "https://www.mylogs.com",
                bufferBaseFileName: "SomeBuffer",
                bufferFileSizeLimitBytes: null,
                bufferFileShared: false,
                retainedBufferFileCountLimit: null,
                logEventLimitBytes: 1, // Is lower than emitted log event
                logEventsInBatchLimit: 1000,
                batchSizeLimitBytes: null,
                period: TimeSpan.FromMilliseconds(1), // 1 ms period
                textFormatter: new NormalTextFormatter(),
                batchFormatter: new DefaultBatchFormatter(),
                httpClient: httpClient);

            // Act
            sink.Emit(Some.InformationEvent());

            await Task.Delay(TimeSpan.FromSeconds(10)); // Sleep 10000x the period

            // Assert
            httpClient.BatchCount.ShouldBe(0);
            httpClient.LogEvents.ShouldBeEmpty();
        }
    }
}
