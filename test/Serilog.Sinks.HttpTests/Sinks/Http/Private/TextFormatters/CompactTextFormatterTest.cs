using System;
using System.IO;
using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.TextFormatters
{
    public class CompactTextFormatterTest
    {
        private readonly StringWriter output;

        private ILogger logger;

        public CompactTextFormatterTest()
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
            logger = CreateLogger(new CompactRenderedTextFormatter());

            // Act
            logger.Write(level, "No properties");

            // Assert
            var @event = GetEvent();

            if (level == LogEventLevel.Information)
            {
                @event.Level.ShouldBeNull();
            }
            else
            {
                @event.Level.ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyEvent(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("No properties");

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.Level.ShouldBeNull();
            @event.MessageTemplate.ShouldBe("No properties");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "No properties" : null);
            @event.Exception.ShouldBeNull();
            @event.Renderings.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MinimalEvent(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("One {Property}");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "One 42" : null);
            @event.Exception.ShouldBeNull();
            GetProperty("Property").ShouldBe("42");
            @event.Renderings.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MultipleProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("Property {First} and {Second}");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            @event.Exception.ShouldBeNull();
            GetProperty("First").ShouldBe("One");
            GetProperty("Second").ShouldBe("Two");
            @event.Renderings.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exceptions(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("With exception");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "With exception" : null);
            @event.Exception.ShouldNotBeNull();
            @event.Renderings.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionAndProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("With exception and {Property}");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            @event.Exception.ShouldNotBeNull();
            GetProperty("Property").ShouldBe("42");
            @event.Renderings.ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Renderings(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("One {Rendering:x8}");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            @event.Exception.ShouldBeNull();
            GetProperty("Rendering").ShouldBe("42");
            @event.Renderings.ShouldBe(new[] { "0000002a" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MultipleRenderings(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var @event = GetEvent();
            @event.Timestamp.ShouldNotBeNull();
            @event.MessageTemplate.ShouldBe("Rendering {First:x8} and {Second:x8}");
            @event.RenderedMessage.ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            @event.Exception.ShouldBeNull();
            GetProperty("First").ShouldBe("1");
            GetProperty("Second").ShouldBe("2");
            @event.Renderings.ShouldBe(new[] { "00000001", "00000002" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NastyException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new NastyException(), "With exception");

            // Assert
            output.ToString().ShouldBe(string.Empty);
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
