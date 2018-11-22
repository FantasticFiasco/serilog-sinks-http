using System;
using System.Threading.Tasks;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
{
    public class TimeRolledDurableHttpSinkShould
    {
        [Theory]
        [InlineData("a{Date}b")]
        [InlineData("a{Hour}b")]
        [InlineData("a{HalfHour}b")]
        [InlineData("aa{Date}bb")]
        [InlineData("aa{Hour}bb")]
        [InlineData("aa{HalfHour}bb")]
        [InlineData("aaa{Date}bbb")]
        [InlineData("aaa{Hour}bbb")]
        [InlineData("aaa{HalfHour}bbb")]
        public void ReturnSinkGivenValidBufferPathFormat(string bufferPathFormat)
        {
            // Arrange
            Func<TimeRolledDurableHttpSink> actual = () => new TimeRolledDurableHttpSink(
                "some/route",
                bufferPathFormat,
                null,
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
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("File.json")]
        [InlineData("Date")]
        [InlineData("Hour")]
        [InlineData("HalfHour")]
        [InlineData("{Date}")]
        [InlineData("{Hour}")]
        [InlineData("{HalfHour}")]
        [InlineData("a{Date}")]
        [InlineData("a{Hour}")]
        [InlineData("a{HalfHour}")]
        [InlineData("{Date}b")]
        [InlineData("{Hour}b")]
        [InlineData("{HalfHour}b")]
        [InlineData("a{Date}{Hour}b")]
        [InlineData("a{Date}{HalfHour}b")]
        [InlineData("a{Hour}{HalfHour}b")]
        [InlineData("{date}")]
        [InlineData("{hour}")]
        [InlineData("{halfhour}")]
        [InlineData("a{date}b")]
        [InlineData("a{hour}b")]
        [InlineData("a{halfhour}b")]
        [InlineData(" a{Date}b")]
        [InlineData(" a{Hour}b")]
        [InlineData(" a{HalfHour}b")]
        [InlineData("a{Date}b ")]
        [InlineData("a{Hour}b ")]
        [InlineData("a{HalfHour}b ")]
        [InlineData(" a{Date}b ")]
        [InlineData(" a{Hour}b ")]
        [InlineData(" a{HalfHour}b ")]
        public void ThrowExceptionGivenInvalidBufferPathFormat(string bufferPathFormat)
        {
            // Arrange
            Func<TimeRolledDurableHttpSink> actual = () => new TimeRolledDurableHttpSink(
                "some/route",
                bufferPathFormat,
                null,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalTextFormatter(),
                new ArrayBatchFormatter(),
                new HttpClientMock());

            // Act & Assert
            actual.ShouldThrow<ArgumentException>();
        }

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
            Func<TimeRolledDurableHttpSink> actual = () => new TimeRolledDurableHttpSink(
                "some/route",
                "Buffer-{Date}.json",
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
            Func<TimeRolledDurableHttpSink> actual = () => new TimeRolledDurableHttpSink(
                "some/route",
                "Buffer-{Date}.json",
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

            using (new TimeRolledDurableHttpSink(
                "some/route",
                "Buffer-{Date}.json",
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
