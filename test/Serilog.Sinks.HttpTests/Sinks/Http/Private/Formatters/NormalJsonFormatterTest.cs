using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.IntegrationTests.Server.Controllers.Dto;
using Serilog.Support;
using Xunit;

namespace Serilog.Sinks.Http.Private.Formatters
{
	public class NormalJsonFormatterTest
	{
		private readonly StringWriter output;

		private ILogger logger;

		public NormalJsonFormatterTest()
		{
			output = new StringWriter();
		}

		[Theory]
		[InlineData(LogEventLevel.Verbose)]
		[InlineData(LogEventLevel.Debug)]
		[InlineData(LogEventLevel.Information)]
		[InlineData(LogEventLevel.Warning)]
		[InlineData(LogEventLevel.Error)]
		[InlineData(LogEventLevel.Fatal)]
		public void LogEventLevels(LogEventLevel level)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(true));

			// Act
			logger.Write(level, "No properties");

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Level);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void EmptyEvent(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information("No properties");

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("No properties", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "No properties" : null, @event.RenderedMessage);
			Assert.Null(@event.Exception);
			Assert.Null(@event.Properties);
			Assert.Null(@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void MinimalEvent(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information("One {Property}", 42);

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("One {Property}", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "One 42" : null, @event.RenderedMessage);
			Assert.Null(@event.Exception);
			Assert.Equal(new Dictionary<string, string> { { "Property", "42" } }, @event.Properties);
			Assert.Null(@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void MultipleProperties(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information("Property {First} and {Second}", "One", "Two");

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("Property {First} and {Second}", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "Property \"One\" and \"Two\"" : null, @event.RenderedMessage);
			Assert.Null(@event.Exception);
			Assert.Equal(new Dictionary<string, string> { { "First", "One" }, { "Second", "Two" } }, @event.Properties);
			Assert.Null(@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Exceptions(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information(new DivideByZeroException(), "With exception");

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("With exception", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "With exception" : null, @event.RenderedMessage);
			Assert.NotNull(@event.Exception);
			Assert.Null(@event.Properties);
			Assert.Null(@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ExceptionAndProperties(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("With exception and {Property}", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "With exception and 42" : null, @event.RenderedMessage);
			Assert.NotNull(@event.Exception);
			Assert.Equal(new Dictionary<string, string> { { "Property", "42" } }, @event.Properties);
			Assert.Null(@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Renderings(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information("One {Rendering:x8}", 42);

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("One {Rendering:x8}", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "One 0000002a" : null, @event.RenderedMessage);
			Assert.Null(@event.Exception);
			Assert.Equal(new Dictionary<string, string> { { "Rendering", "42" } }, @event.Properties);
			Assert.Equal(
				new Dictionary<string, RenderingDto[]>
				{
					{
						"Rendering",
						new[] { new RenderingDto { Format = "x8", Rendering = "0000002a" } }
					}
				},
				@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void MultipleRenderings(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

			// Assert
			var @event = GetEvent();
			Assert.NotNull(@event.Timestamp);
			Assert.Equal("Information", @event.Level);
			Assert.Equal("Rendering {First:x8} and {Second:x8}", @event.MessageTemplate);
			Assert.Equal(isRenderingMessage ? "Rendering 00000001 and 00000002" : null, @event.RenderedMessage);
			Assert.Null(@event.Exception);
			Assert.Equal(new Dictionary<string, string> { { "First", "1" }, { "Second", "2" } }, @event.Properties);
			Assert.Equal(
				new Dictionary<string, RenderingDto[]>
				{
					{
						"First",
						new[] { new RenderingDto { Format = "x8", Rendering = "00000001" } }
					},
					{
						"Second",
						new[] { new RenderingDto { Format = "x8", Rendering = "00000002" } }
					}
				},
				@event.Renderings);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void NastyException(bool isRenderingMessage)
		{
			// Arrange
			logger = CreateLogger(new NormalJsonFormatter(isRenderingMessage));

			// Act
			logger.Information(new NastyException(), "With exception");

			// Assert
			Assert.Equal(string.Empty, output.ToString());
		}

		private ILogger CreateLogger(ITextFormatter formatter)
		{
			return new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.Sink(new TextWriterSink(output, formatter))
				.CreateLogger();
		}

		private EventDto GetEvent()
		{
			return JsonConvert.DeserializeObject<EventDto>(output.ToString());
		}
	}
}
