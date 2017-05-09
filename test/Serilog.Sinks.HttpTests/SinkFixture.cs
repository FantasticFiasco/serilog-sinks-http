using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Serilog.LogServer;
using Serilog.Support;
using Xunit;

namespace Serilog
{
	public abstract class SinkFixture : TestServerFixture
	{
		protected Logger Logger { get; set; }

	    protected TestServerHttpClient HttpClient { get; set; }

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
			Logger.Write(level, "Some message");

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
				Logger.Information("Some message");
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
			Logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Equal(expected.Level.ToString(), @event.Level);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Equal(expected.Properties["Name"].ToString().Trim('"'), @event.Properties["Name"]);
			Assert.Equal("Hello, \"Alice\"!", @event.RenderedMessage);
			Assert.Null(@event.Exception);
		}

		[Fact]
		public async Task Exception()
		{
			// Arrange
			var expected = Some.LogEvent(LogEventLevel.Error, new Exception("Some exception"), "Some error message");

			// Act
			Logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Equal(expected.Level.ToString(), @event.Level);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Equal("Some error message", @event.RenderedMessage);
			Assert.Equal(expected.Exception.ToString(), @event.Exception);
		}

		[Fact]
		public async Task DropNastyException()
		{
			// Arrange
			var nastyException = Some.LogEvent(LogEventLevel.Error, new NastyException(), "Some error message");
			var expected = Some.LogEvent("Some message");

			// Act
			Logger.Write(nastyException);
			Logger.Write(expected);

			// Assert
			var @event = (await Api.WaitAndGetAsync(1)).Single();

			Assert.Equal(expected.Timestamp, @event.Timestamp);
			Assert.Equal(expected.Level.ToString(), @event.Level);
			Assert.Equal(expected.MessageTemplate.Text, @event.MessageTemplate);
			Assert.Null(@event.Exception);
		}

		[Fact]
		public async Task NetworkFailure()
		{
			// Arrange
			HttpClient.SimulateNetworkFailure();

			// Act
			Logger.Write(LogEventLevel.Information, "Some message");

			// Assert
			await Api.WaitAndGetAsync(1);
			Assert.Equal(2, HttpClient.NumberOfPosts);
		}

		public override void Dispose()
		{
			base.Dispose();

			Logger?.Dispose();
			HttpClient?.Dispose();
		}
	}
}
