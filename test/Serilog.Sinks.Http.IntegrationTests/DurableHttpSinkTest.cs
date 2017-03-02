using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Http.IntegrationTests.Support;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
	public class DurableHttpSinkTest : TestServerFixture, IDisposable
	{
		private readonly Logger logger;
		
		public DurableHttpSinkTest()
		{
			ClearBufferFiles();

			logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.DurableHttp(
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
		[InlineData(10000)]     // ~100 batches
		public async Task Batches(int numberOfEvents)
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
		public async Task Payload()
		{
			// Arrange
			var expected = Some.LogEvent("Hello, {Name}!", "Alice");

			// Act
			logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Null(@event.Level);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Null(@event.Exception);
		}

		[Fact]
		public async Task Exception()
		{
			// Arrange
			var expected = Some.LogEvent(LogEventLevel.Error, new Exception("Some exception"), "Some error message");

			// Act
			logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Equal(expected.Level.ToString(), @event.Level);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Equal(expected.Exception.ToString(), @event.Exception);
		}

		[Fact]
		public async Task DropNastyException()
		{
			// Arrange
			var nasty = Some.LogEvent(LogEventLevel.Error, new NastyException(), "Some error message");
			var expected = Some.LogEvent("Some message");

			// Act
			logger.Write(nasty);
			logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Null(@event.Exception);
		}

		void IDisposable.Dispose()
		{
			logger?.Dispose();
		}
	}
}
