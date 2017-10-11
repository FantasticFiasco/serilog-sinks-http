using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Xunit;

namespace Serilog.Sinks.Http.Private.Sinks
{
    public class HttpSinkTest
    {
        [Fact]
        public async Task NoNetworkTrafficWithoutLogEvents()
        {
            // Arrange
            var httpClient = new Mock<IHttpClient>();

            // ReSharper disable once UnusedVariable
            var httpSink = new HttpSink(
                "api/events",
                1000,
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                httpClient.Object);

            // Act
            await Task.Delay(TimeSpan.FromMinutes(3));
            
            // Assert
            httpClient.Verify(
                mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()),
                Times.Never);
        }
    }
}
