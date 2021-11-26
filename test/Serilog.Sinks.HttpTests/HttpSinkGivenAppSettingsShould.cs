using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.Private.NonDurable;
using Serilog.Support.Fixtures;
using Serilog.Support.Reflection;
using Xunit;

namespace Serilog
{
    public class HttpSinkGivenAppSettingsShould : IClassFixture<WebServerFixture>
    {
        private readonly WebServerFixture webServerFixture;

        public HttpSinkGivenAppSettingsShould(WebServerFixture webServerFixture)
        {
            this.webServerFixture = webServerFixture;
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public async Task WriteLogEvent(LogEventLevel level)
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var testId = $"WriteLogEvent_{Guid.NewGuid()}";

            new HttpSinkReflection(logger.GetSink<HttpSink>())
                .SetRequestUri(webServerFixture.RequestUri(testId))
                .SetHttpClient(new JsonHttpClient(webServerFixture.CreateClient()));

            // Act
            logger.Write(level, "Some message");

            // Assert
            await webServerFixture.ExpectLogEvents(testId, 1);
        }

        [Theory]
        [InlineData(1)]          // 1 batch assuming batch size is 100
        [InlineData(10)]         // 1 batch assuming batch size is 100
        [InlineData(100)]        // ~1 batch assuming batch size is 100
        [InlineData(1_000)]      // ~10 batches assuming batch size is 100
        [InlineData(10_000)]     // ~100 batches assuming batch size is 100
        public async Task WriteBatches(int numberOfEvents)
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var testId = $"WriteLogEvent_{Guid.NewGuid()}";

            new HttpSinkReflection(logger.GetSink<HttpSink>())
                .SetRequestUri(webServerFixture.RequestUri(testId))
                .SetHttpClient(new JsonHttpClient(webServerFixture.CreateClient()));

            // Act
            for (int i = 0; i < numberOfEvents; i++)
            {
                logger.Information("Some message");
            }

            // Assert
            await webServerFixture.ExpectLogEvents(
                testId,
                numberOfEvents,
                TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task OvercomeNetworkFailure()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings_http.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var testId = $"WriteLogEvent_{Guid.NewGuid()}";

            new HttpSinkReflection(logger.GetSink<HttpSink>())
                .SetRequestUri(webServerFixture.RequestUri(testId))
                .SetHttpClient(new JsonHttpClient(webServerFixture.CreateClient()));

            webServerFixture.SimulateNetworkFailure(TimeSpan.FromSeconds(5));

            // Act
            logger.Write(LogEventLevel.Information, "Some message");

            // Assert
            await webServerFixture.ExpectBatches(testId, 1, TimeSpan.FromSeconds(30));
            await webServerFixture.ExpectLogEvents(testId, 1, TimeSpan.FromSeconds(30));
        }
    }
}
