using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Support;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.TextFormatters
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
        public void Level(LogEventLevel level)
        {
            // Arrange
            logger = CreateLogger(new CompactRenderedTextFormatter());

            // Act
            logger.Write(level, "No properties");

            // Assert
            var @event = GetEvent();

            if (level == LogEventLevel.Information)
            {
                @event["@l"].ShouldBeNull();
            }
            else
            {
                @event["@l"].ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Message(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("No properties");

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@l"].ShouldBeNull();
            @event["@mt"].ShouldBe("No properties");
            @event["@m"].ShouldBe(isRenderingMessage ? "No properties" : null);
            @event["@x"].ShouldBeNull();
            @event["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertyInMessageTemplate(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("One {Property}");
            @event["@m"].ShouldBe(isRenderingMessage ? "One 42" : null);
            @event["@x"].ShouldBeNull();
            @event["Property"].ShouldBe(42);
            @event["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertiesInMessageTemplate(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("Property {First} and {Second}");
            @event["@m"].ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            @event["@x"].ShouldBeNull();
            @event["First"].ShouldBe("One");
            @event["Second"].ShouldBe("Two");
            @event["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnrichedProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger
                .ForContext("First", "One")
                .ForContext("Second", "Two")
                .Information("No properties");

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("No properties");
            @event["@m"].ShouldBe(isRenderingMessage ? "No properties" : null);
            @event["@x"].ShouldBeNull();
            @event["First"].ShouldBe("One");
            @event["Second"].ShouldBe("Two");
            @event["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Rendering(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("One {Rendering:x8}");
            @event["@m"].ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            @event["@x"].ShouldBeNull();
            @event["Rendering"].ShouldBe(42);
            @event["@r"].Select(token => token.Value<string>()).ShouldBe(new[] { "0000002a" });
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
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            @event["@m"].ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            @event["@x"].ShouldBeNull();
            @event["First"].ShouldBe(1);
            @event["Second"].ShouldBe(2);
            @event["@r"].Children().Select(token => token.Value<string>()).ShouldBe(new[] { "00000001", "00000002" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exception(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("With exception");
            @event["@m"].ShouldBe(isRenderingMessage ? "With exception" : null);
            @event["@x"].ShouldNotBeNull();
            @event["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionAndProperty(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["@t"].ShouldNotBeNull();
            @event["@mt"].ShouldBe("With exception and {Property}");
            @event["@m"].ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            @event["@x"].ShouldNotBeNull();
            @event["Property"].ShouldBe(42);
            @event["@r"].ShouldBeNull();
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

        private JObject GetEvent()
        {
            return JObject.Parse(output.ToString());
        }
    }
}
