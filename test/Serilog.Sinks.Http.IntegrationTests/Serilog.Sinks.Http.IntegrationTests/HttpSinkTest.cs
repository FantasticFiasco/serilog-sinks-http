using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Http.IntegrationTests.Support;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class HttpSinkTest : TestServerFixture, IDisposable
    {
	    private readonly Logger logger;
		
		public HttpSinkTest()
		{
			ClearBufferFiles();

			logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.Http(
					"api/batches",
					BufferBaseFilename,
					batchPostingLimit: 100,
					period: TimeSpan.FromMilliseconds(1),
					httpClient: new TestServerHttpClient(Server.CreateClient()))
				.CreateLogger();
		}

		[Theory]
		[InlineData(LogEventLevel.Verbose)]
		[InlineData(LogEventLevel.Debug)]
		[InlineData(LogEventLevel.Information)]
		[InlineData(LogEventLevel.Warning)]
		[InlineData(LogEventLevel.Error)]
		[InlineData(LogEventLevel.Fatal)]
		public async Task Level(LogEventLevel level)
		{
			// Act
			logger.Write(level, "Some message");

			// Assert
			await Api.WaitAndGetAsync(1);
		}

		[Theory]
		[InlineData(1)]         // 1 batch
		[InlineData(10)]        // 1 batch
		[InlineData(100)]       // ~1 batch
		[InlineData(1000)]      // ~10 batches
		public async Task Emit(int numberOfEvents)
		{
			// Act
			for (int i = 0; i < numberOfEvents; i++)
			{
				logger.Information("Some message");
			}

			// Assert
			await Api.WaitAndGetAsync(numberOfEvents);
		}

		[Fact]
		public async Task EventsAreFormattedIntoJsonPayloads()
		{
			// Arrange
			var expected = Some.LogEvent("Hello, {Name}!", "Alice");

			// Act
			logger.Write(expected);

			// Assert
			var events = await Api.WaitAndGetAsync(1);

			Assert.Equal(expected.RenderMessage(), events.First().RenderedMessage);
		}

		void IDisposable.Dispose()
	    {
		    logger?.Dispose();
	    }
    }
}
