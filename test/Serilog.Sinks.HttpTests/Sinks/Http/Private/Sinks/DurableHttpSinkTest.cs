using System;
using Serilog.LogServer;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
{
    public class DurableHttpSinkTest
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
        public void ValidBufferPathFormat(string bufferPathFormat)
        {
            // Arrange
            Action provider = () => new DurableHttpSink(
                "api/events",
                bufferPathFormat,
                null,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                new TestServerHttpClient());

            // Assert
            provider.ShouldNotThrow();
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
        public void InvalidBufferPathFormat(string bufferPathFormat)
        {
            // Arrange
            Action provider = () => new DurableHttpSink(
                "api/events",
                bufferPathFormat,
                null,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                new TestServerHttpClient());

            // Assert
            provider.ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void ValidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Action provider = () => new DurableHttpSink(
                "api/events",
                "Buffer-{Date}.json",
                bufferFileSizeLimitBytes,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                new TestServerHttpClient());

            // Assert
            provider.ShouldNotThrow();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void InvalidBufferFileSizeLimitBytes(int? bufferFileSizeLimitBytes)
        {
            // Arrange
            Action provider = () => new DurableHttpSink(
                "api/events",
                "Buffer-{Date}.json",
                bufferFileSizeLimitBytes,
                31,
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                new TestServerHttpClient());

            // Assert
            provider.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
