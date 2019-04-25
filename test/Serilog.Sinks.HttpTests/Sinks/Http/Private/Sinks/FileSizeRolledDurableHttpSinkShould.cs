using System;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
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
            Func<FileSizeRolledDurableHttpSink> actual = () => new FileSizeRolledDurableHttpSink(
                "some/route",
                "Buffer",
                bufferFileSizeLimitBytes,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                new HttpClientMock());

            // Act & Assert
            actual.ShouldNotThrow();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void ThrowExceptionGivenInvalidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Func<FileSizeRolledDurableHttpSink> actual = () => new FileSizeRolledDurableHttpSink(
                "some/route",
                "Buffer",
                bufferFileSizeLimitBytes,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                new HttpClientMock());

            // Act & Assert
            actual.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task StayIdleGivenNoLogEvents()
        {
            // Arrange
            var httpClient = new HttpClientMock();

            using (new FileSizeRolledDurableHttpSink(
                "some/route",
                "Buffer",
                null,
                null,
                1,
                TimeSpan.FromMilliseconds(1),         // 1 ms period
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                httpClient))
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
