using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
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

        [Fact]
        public async Task RespectQueueLimit()
        {
            // Arrange
            var httpClient = new Mock<IHttpClient>();

            var httpSink = new HttpSink(
                "api/events",
                1,
                1,  // Queue only holds 1 event
                TimeSpan.FromSeconds(2),
                new NormalRenderedTextFormatter(),
                new DefaultBatchFormatter(),
                httpClient.Object);

            // Act
            for (int i = 0; i < 10; i++)
            {
                httpSink.Emit(Some.InformationEvent());
            }

            await Task.Delay(TimeSpan.FromSeconds(4));

            // Assert
            httpClient.Verify(
                mock => mock.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()),
                Times.Exactly(2)); // Two since the first and the last event will be sent, all other will be dropped from the queue
        }
    }
}
