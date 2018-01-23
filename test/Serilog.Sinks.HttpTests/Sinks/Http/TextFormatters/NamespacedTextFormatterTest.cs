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
    public class NamespacedTextFormatterTest
    {
        private readonly StringWriter output;

        private ILogger logger;

        public NamespacedTextFormatterTest()
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
            logger = CreateLogger(new Formatter("Foo", "Bar", true));

            // Act
            logger.Write(level, "No properties");

            // Assert
            var @event = GetEvent();
            @event["Level"].ShouldNotBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Message(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("No properties");

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("No properties");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertyInMessageTemplateUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("One {Property}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].Children().Count().ShouldBe(1);
            @event["Foo"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertyInMessageTemplateUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("One {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("One {Property}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 42" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"]["Bar"].Children().Count().ShouldBe(1);
            @event["Foo"]["Bar"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertiesInMessageTemplateUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("Property {First} and {Second}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].Children().Count().ShouldBe(2);
            @event["Foo"]["First"].ShouldBe("One");
            @event["Foo"]["Second"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PropertiesInMessageTemplateUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("Property {First} and {Second}", "One", "Two");

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("Property {First} and {Second}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"]["Bar"].Children().Count().ShouldBe(2);
            @event["Foo"]["Bar"]["First"].ShouldBe("One");
            @event["Foo"]["Bar"]["Second"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EnrichedProperties(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger
                .ForContext("First", "One")
                .ForContext("Second", "Two")
                .Information("No properties");

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["MessageTemplate"].ShouldBe("No properties");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].Children().Count().ShouldBe(2);
            @event["Properties"]["First"].ShouldBe("One");
            @event["Properties"]["Second"].ShouldBe("Two");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RenderingUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("One {Rendering:x8}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].Children().Count().ShouldBe(2);
            @event["Foo"]["Rendering"].ShouldBe(42);
            @event["Foo"]["Renderings"]["Rendering"].Children().Count().ShouldBe(1);
            @event["Foo"]["Renderings"]["Rendering"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Renderings"]["Rendering"][0]["Rendering"].ShouldBe("0000002a");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RenderingUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("One {Rendering:x8}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("One {Rendering:x8}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "One 0000002a" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"]["Bar"].Children().Count().ShouldBe(2);
            @event["Foo"]["Bar"]["Rendering"].ShouldBe(42);
            @event["Foo"]["Bar"]["Renderings"]["Rendering"].Children().Count().ShouldBe(1);
            @event["Foo"]["Bar"]["Renderings"]["Rendering"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Bar"]["Renderings"]["Rendering"][0]["Rendering"].ShouldBe("0000002a");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RenderingsUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].Children().Count().ShouldBe(3);
            @event["Foo"]["First"].ShouldBe(1);
            @event["Foo"]["Second"].ShouldBe(2);
            @event["Foo"]["Renderings"]["First"].Children().Count().ShouldBe(1);
            @event["Foo"]["Renderings"]["First"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Renderings"]["First"][0]["Rendering"].ShouldBe("00000001");
            @event["Foo"]["Renderings"]["Second"].Children().Count().ShouldBe(1);
            @event["Foo"]["Renderings"]["Second"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Renderings"]["Second"][0]["Rendering"].ShouldBe("00000002");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RenderingsUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("Rendering {First:x8} and {Second:x8}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
            @event["Exception"].ShouldBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"]["Bar"].Children().Count().ShouldBe(3);
            @event["Foo"]["Bar"]["First"].ShouldBe(1);
            @event["Foo"]["Bar"]["Second"].ShouldBe(2);
            @event["Foo"]["Bar"]["Renderings"]["First"].Children().Count().ShouldBe(1);
            @event["Foo"]["Bar"]["Renderings"]["First"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Bar"]["Renderings"]["First"][0]["Rendering"].ShouldBe("00000001");
            @event["Foo"]["Bar"]["Renderings"]["Second"].Children().Count().ShouldBe(1);
            @event["Foo"]["Bar"]["Renderings"]["Second"][0]["Format"].ShouldBe("x8");
            @event["Foo"]["Bar"]["Renderings"]["Second"][0]["Rendering"].ShouldBe("00000002");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exception(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception");

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("With exception");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception" : null);
            @event["Exception"].ShouldNotBeNull();
            @event["Properties"].ShouldBeNull();
            @event["Foo"].ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionAndPropertyUsingComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", null, isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("With exception and {Property}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            @event["Exception"].ShouldNotBeNull();
            @event["Foo"].Children().Count().ShouldBe(1);
            @event["Foo"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExceptionAndPropertyUsingSubComponentNamespace(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

            // Act
            logger.Information(new DivideByZeroException(), "With exception and {Property}", 42);

            // Assert
            var @event = GetEvent();
            @event["Timestamp"].ShouldNotBeNull();
            @event["Level"].ShouldBe("Information");
            @event["MessageTemplate"].ShouldBe("With exception and {Property}");
            ((string)@event["RenderedMessage"]).ShouldBe(isRenderingMessage ? "With exception and 42" : null);
            @event["Exception"].ShouldNotBeNull();
            @event["Foo"]["Bar"].Children().Count().ShouldBe(1);
            @event["Foo"]["Bar"]["Property"].ShouldBe(42);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NastyException(bool isRenderingMessage)
        {
            // Arrange
            logger = CreateLogger(new Formatter("Foo", "Bar", isRenderingMessage));

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

        private class Formatter : NamespacedTextFormatter
        {
            public Formatter(string component, string subComponent, bool isRenderingMessage)
            {
                Component = component;
                SubComponent = subComponent;
                IsRenderingMessage = isRenderingMessage;
            }

            protected override string Component { get; }

            protected override string SubComponent { get; }

            protected override bool IsRenderingMessage { get; }
        }
    }
}
