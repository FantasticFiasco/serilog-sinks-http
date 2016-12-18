using System;
using System.Net.Http;
using Moq;
using Serilog.Sinks.Http.Tests.Support;
using Xunit;

namespace Serilog.Sinks.Http.Tests.Sinks.Http
{
    public class HttpSinkTest
    {
        private readonly Mock<IHttpClient> client;
        private readonly string requestUri;
        private readonly HttpSink sink;

        public HttpSinkTest()
        {
            client = new Mock<IHttpClient>();
            requestUri = "www.mylogs.com";
            sink = new HttpSink(
                client.Object,
                requestUri,
                HttpSink.DefaultBatchPostingLimit,
                HttpSink.DefaultPeriod,
                null);
        }

        [Fact]
        public void RequestUri()
        {
            // Arrange
            var counter = new Counter(1);

            client
                .Setup(mock => mock.PostAsync(requestUri, It.IsAny<HttpContent>()))
                .Callback(() => counter.Increment());

            // Act
            sink.Emit(Some.DebugEvent());


            // Assert
            counter.Wait(TimeSpan.FromSeconds(10));
        }
    }
}
