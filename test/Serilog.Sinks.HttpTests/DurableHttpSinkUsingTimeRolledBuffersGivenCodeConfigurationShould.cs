using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Events;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.HttpClients;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Serilog.Support.Fixtures;
using Shouldly;
using Xunit;

namespace Serilog;

public class DurableHttpSinkUsingTimeRolledBuffersGivenCodeConfigurationShould
    : IClassFixture<WebServerFixture>
{
    private readonly WebServerFixture webServerFixture;

    public DurableHttpSinkUsingTimeRolledBuffersGivenCodeConfigurationShould(WebServerFixture webServerFixture)
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
        var testId = $"WriteLogEvent_{Guid.NewGuid()}";

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo
            .DurableHttpUsingTimeRolledBuffers(
                requestUri: webServerFixture.RequestUri(testId),
                bufferBaseFileName: Path.Combine("logs", testId),
                bufferRollingInterval: BufferRollingInterval.Hour,
                logEventsInBatchLimit: 100,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromMilliseconds(1),
                textFormatter: new NormalRenderedTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient()))
            .CreateLogger();

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
        var testId = $"WriteBatches_{Guid.NewGuid()}";

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo
            .DurableHttpUsingTimeRolledBuffers(
                requestUri: webServerFixture.RequestUri(testId),
                bufferBaseFileName: Path.Combine("logs", testId),
                logEventsInBatchLimit: 100,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromMilliseconds(1),
                textFormatter: new NormalRenderedTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient()))
            .CreateLogger();

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
        var testId = $"OvercomeNetworkFailure_{Guid.NewGuid()}";

        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo
            .DurableHttpUsingTimeRolledBuffers(
                requestUri: webServerFixture.RequestUri(testId),
                bufferBaseFileName: Path.Combine("logs", testId),
                logEventsInBatchLimit: 100,
                batchSizeLimitBytes: ByteSize.MB,
                period: TimeSpan.FromMilliseconds(1),
                textFormatter: new NormalRenderedTextFormatter(),
                batchFormatter: new ArrayBatchFormatter(),
                httpClient: new JsonHttpClient(webServerFixture.CreateClient()))
            .CreateLogger();

        webServerFixture.SimulateNetworkFailure(TimeSpan.FromSeconds(5));

        // Act
        logger.Write(LogEventLevel.Information, "Some message");

        // Assert
        await webServerFixture.ExpectBatches(testId, 1, TimeSpan.FromSeconds(30));
        await webServerFixture.ExpectLogEvents(testId, 1, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void ConfigureHttpClient()
    {
        // Arrange
        var httpClient = new HttpClientMock();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo
            .DurableHttpUsingTimeRolledBuffers(
                requestUri: "https://www.mylogs.com",
                httpClient: httpClient,
                configuration: configuration)
            .CreateLogger();

        // Assert
        httpClient.Configuration.ShouldBe(configuration);
    }
}