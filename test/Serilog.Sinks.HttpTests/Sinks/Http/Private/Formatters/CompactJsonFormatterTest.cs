using System;
using System.IO;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;
using Serilog.Support;
using Xunit;

namespace Serilog.Sinks.Http.Private.Formatters
{
    public class CompactJsonFormatterTest
    {
        private readonly StringWriter output;

        private ILogger logger;

        public CompactJsonFormatterTest()
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
            logger = CreateLogger(new CompactJsonFormatter(true));

            // Act
            logger.Write(level, "No properties");

            // Assert
            var @event = GetEvent();

            if (level == LogEventLevel.Information)
            {
                Assert.Null(@event.Level);
            }
            else
            {
                Assert.NotNull(@event.Level);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyEvent(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information("No properties");

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Null(@event.Level);
            Assert.Equal("No properties", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "No properties" : null, @event.RenderedMessage);
            Assert.Null(@event.Exception);
            Assert.Null(@event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MinimalEvent(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("One {Property}", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "One 42" : null, @event.RenderedMessage);
            Assert.Null(@event.Exception);
            Assert.Equal("42", GetProperty("Property"));
            Assert.Null(@event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MultipleProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("Property {First} and {Second}", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "Property \"One\" and \"Two\"" : null, @event.RenderedMessage);
            Assert.Null(@event.Exception);
            Assert.Equal("One", GetProperty("First"));
            Assert.Equal("Two", GetProperty("Second"));
            Assert.Null(@event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exceptions(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("With exception", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "With exception" : null, @event.RenderedMessage);
            Assert.NotNull(@event.Exception);
            Assert.Null(@event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionAndProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("With exception and {Property}", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "With exception and 42" : null, @event.RenderedMessage);
            Assert.NotNull(@event.Exception);
            Assert.Equal("42", GetProperty("Property"));
            Assert.Null(@event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Renderings(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("One {Rendering:x8}", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "One 0000002a" : null, @event.RenderedMessage);
            Assert.Null(@event.Exception);
            Assert.Equal("42", GetProperty("Rendering"));
            Assert.Equal(new[] { "0000002a" }, @event.Renderings);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MultipleRenderings(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new CompactJsonFormatter(isRenderingMessage));

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var @event = GetEvent();
            Assert.NotNull(@event.Timestamp);
            Assert.Equal("Rendering {First:x8} and {Second:x8}", @event.MessageTemplate);
            Assert.Equal(isRenderingMessage ? "Rendering 00000001 and 00000002" : null, @event.RenderedMessage);
            Assert.Null(@event.Exception);
            Assert.Equal("1", GetProperty("First"));
            Assert.Equal("2", GetProperty("Second"));
            Assert.Equal(new[] { "00000001", "00000002" }, @event.Renderings);
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

        private CompactEventDto GetEvent()
        {
            return JsonConvert.DeserializeObject<CompactEventDto>(output.ToString());
        }

        private string GetProperty(string name)
        {
            dynamic @event = JsonConvert.DeserializeObject(output.ToString());
            return @event[name];
        }
    }
}
