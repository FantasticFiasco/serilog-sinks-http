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
    public class NamespacedTextFormatterShould
    {
        private readonly StringWriter output;

        private ILogger logger;

        public NamespacedTextFormatterShould()
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
        public void WriteLevel(LogEventLevel level)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", true));

            // Act
            logger.Write(level, "No properties");

            // Assert
            var logEvent = GetEvent();

            logEvent["Level"].ShouldNotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteMessage(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("No properties");

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("No properties");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertyInMessageTemplateUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("One {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertyInMessageTemplateUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("One {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertiesInMessageTemplateUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("Property {First} and {Second}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["Foo"]["First"].ShouldBe("One");
            logEvent["Properties"]["Foo"]["Second"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertiesInMessageTemplateUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("Property {First} and {Second}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["Foo"]["Bar"]["First"].ShouldBe("One");
            logEvent["Properties"]["Foo"]["Bar"]["Second"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteEnrichedProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger
                .ForContext("FirstContext", "One")
                .ForContext("SecondContext", "Two")
                .Information("No properties");

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["MessageTemplate"].ShouldBe("No properties");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["FirstContext"].ShouldBe("One");
            logEvent["Properties"]["SecondContext"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertyInMessageTemplateAndEnrichedPropertyUsingComponentNamespace(bool isRenderingMessage)
        {

            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger
                .ForContext("FirstContext", "One")
                .Information("One {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["MessageTemplate"].ShouldBe("One {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["FirstContext"].ShouldBe("One");
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WritePropertyInMessageTemplateAndEnrichedPropertyUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger
                .ForContext("FirstContext", "One")
                .Information("One {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["MessageTemplate"].ShouldBe("One {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["FirstContext"].ShouldBe("One");
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRenderingUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("One {Rendering:x8}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["Foo"]["Rendering"].ShouldBe(42);
            logEvent["Properties"]["Foo"]["Renderings"]["Rendering"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Renderings"]["Rendering"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Renderings"]["Rendering"][0]["Rendering"].ShouldBe("0000002a");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRenderingUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("One {Rendering:x8}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(2);
            logEvent["Properties"]["Foo"]["Bar"]["Rendering"].ShouldBe(42);
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Rendering"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Rendering"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Rendering"][0]["Rendering"].ShouldBe("0000002a");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRenderingsUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(3);
            logEvent["Properties"]["Foo"]["First"].ShouldBe(1);
            logEvent["Properties"]["Foo"]["Second"].ShouldBe(2);
            logEvent["Properties"]["Foo"]["Renderings"]["First"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Renderings"]["First"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Renderings"]["First"][0]["Rendering"].ShouldBe("00000001");
            logEvent["Properties"]["Foo"]["Renderings"]["Second"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Renderings"]["Second"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Renderings"]["Second"][0]["Rendering"].ShouldBe("00000002");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteRenderingsUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            logEvent["Exception"].ShouldBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(3);
            logEvent["Properties"]["Foo"]["Bar"]["First"].ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Second"].ShouldBe(2);
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["First"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["First"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["First"][0]["Rendering"].ShouldBe("00000001");
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Second"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Second"][0]["Format"].ShouldBe("x8");
            logEvent["Properties"]["Foo"]["Bar"]["Renderings"]["Second"][0]["Rendering"].ShouldBe("00000002");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("With exception");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception" : null);
            logEvent["Exception"].ShouldNotBeNull();
            logEvent["Properties"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteExceptionAndPropertyUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("With exception and {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            logEvent["Exception"].ShouldNotBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteExceptionAndPropertyUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var logEvent = GetEvent();

            logEvent["Timestamp"].ShouldNotBeNull();
            logEvent["Level"].ShouldBe("Information");
            logEvent["MessageTemplate"].ShouldBe("With exception and {Property}");
            ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            logEvent["Exception"].ShouldNotBeNull();
            logEvent["Properties"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"].Children().Count().ShouldBe(1);
            logEvent["Properties"]["Foo"]["Bar"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SkipNastyException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

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

        private class Formatter : NamespacedTextFormatter
        {
            public Formatter(string component, string subComponent, bool isRenderingMessage)
                : base(component, subComponent)
            {
                IsRenderingMessage = isRenderingMessage;
            }
        }
    }
}
