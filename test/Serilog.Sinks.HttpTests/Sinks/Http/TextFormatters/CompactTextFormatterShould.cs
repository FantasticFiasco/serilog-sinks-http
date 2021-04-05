using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Support;
using Serilog.Support.Sinks;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.TextFormatters
{
    public class CompactTextFormatterShould
    {
        private readonly StringWriter output;

        private ILogger logger;

        public CompactTextFormatterShould()
        {
            output = new StringWriter();
        }

        [Fact]
        public void Dummy()
        {
            "d".ShouldBeNull();
        }

        [Theory]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public void WriteLevel(LogEventLevel level)
        {
            // Arrange
            logger = CreateLogger(new CompactTextFormatter());

            // Act
            logger.Write(level, "No properties");

            // Assert
            var logEvent = GetEvent();

            if (level == LogEventLevel.Information)
            {
                logEvent["@l"].ShouldBeNull();
            }
            else
            {
                logEvent["@l"].ShouldNotBeNull();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteMessage(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("No properties");

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@l"].ShouldBeNull();
            logEvent["@mt"].ShouldBe("No properties");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertyInMessageTemplate(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("One {Property}");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["Property"].ShouldBe(42);
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertiesInMessageTemplate(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("Property {First} and {Second}");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["First"].ShouldBe("One");
            logEvent["Second"].ShouldBe("Two");
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteEnrichedProperties(bool isRenderingMessage)
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
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("No properties");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["First"].ShouldBe("One");
            logEvent["Second"].ShouldBe("Two");
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRendering(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("One {Rendering:x8}");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["Rendering"].ShouldBe(42);
            logEvent["@r"].Select(token => token.Value<string>()).ShouldBe(new[] { "0000002a" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRenderings(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            logEvent["@x"].ShouldBeNull();
            logEvent["First"].ShouldBe(1);
            logEvent["Second"].ShouldBe(2);
            logEvent["@r"].Children().Select(token => token.Value<string>()).ShouldBe(new[] { "00000001", "00000002" });
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("With exception");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "With exception" : null);
            logEvent["@x"].ShouldNotBeNull();
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteExceptionAndProperty(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["@t"].ShouldNotBeNull();
            logEvent["@mt"].ShouldBe("With exception and {Property}");
            ((string)logEvent["@m"]).ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            logEvent["@x"].ShouldNotBeNull();
            logEvent["Property"].ShouldBe(42);
            logEvent["@r"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SkipNastyException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(isRenderingMessage ?
                new CompactRenderedTextFormatter() :
                new CompactTextFormatter());

            // Act
            logger.Information(new NastyException(), "With exception");

            // Assert
            output.ToString().ShouldBeEmpty();
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
