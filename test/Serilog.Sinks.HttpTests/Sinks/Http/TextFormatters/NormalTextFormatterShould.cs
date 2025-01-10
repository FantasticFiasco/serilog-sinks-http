using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Support;
using Serilog.Support.Sinks;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.TextFormatters;

public class NormalTextFormatterShould
{
    private readonly StringWriter output;

    private ILogger logger;

    public NormalTextFormatterShould()
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
        logger = CreateLogger(new NormalRenderedTextFormatter());

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
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

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
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WritePropertyInMessageTemplate(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

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
        logEvent["Properties"]["Property"].ShouldBe(42);
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WritePropertiesInMessageTemplate(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

        // Act
        logger.Information("Property {First} and {Second}", "One", "Two");

        // Assert
        var logEvent = GetEvent();

        logEvent["Timestamp"].ShouldNotBeNull();
        logEvent["Level"].ShouldBe("Information");
        logEvent["MessageTemplate"].ShouldBe("Property {First} and {Second}");
        ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Property \"One\" and \"Two\"" : null);
        logEvent["Exception"].ShouldBeNull();
        logEvent["Properties"].Children().Count().ShouldBe(2);
        logEvent["Properties"]["First"].ShouldBe("One");
        logEvent["Properties"]["Second"].ShouldBe("Two");
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteEnrichedProperties(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

        // Act
        logger
            .ForContext("First", "One")
            .ForContext("Second", "Two")
            .Information("No properties");

        // Assert
        var logEvent = GetEvent();

        logEvent["Timestamp"].ShouldNotBeNull();
        logEvent["MessageTemplate"].ShouldBe("No properties");
        ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
        logEvent["Exception"].ShouldBeNull();
        logEvent["Properties"].Children().Count().ShouldBe(2);
        logEvent["Properties"]["First"].ShouldBe("One");
        logEvent["Properties"]["Second"].ShouldBe("Two");
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteRendering(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

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
        logEvent["Properties"]["Rendering"].ShouldBe(42);
        logEvent["Renderings"]["Rendering"].Children().Count().ShouldBe(1);
        logEvent["Renderings"]["Rendering"][0]["Format"].ShouldBe("x8");
        logEvent["Renderings"]["Rendering"][0]["Rendering"].ShouldBe("0000002a");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteRenderings(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

        // Act
        logger.Information("Rendering {First:x8} and {Second:x8}", 1, 2);

        // Assert
        var logEvent = GetEvent();

        logEvent["Timestamp"].ShouldNotBeNull();
        logEvent["Level"].ShouldBe("Information");
        logEvent["MessageTemplate"].ShouldBe("Rendering {First:x8} and {Second:x8}");
        ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "Rendering 00000001 and 00000002" : null);
        logEvent["Exception"].ShouldBeNull();
        logEvent["Properties"].Children().Count().ShouldBe(2);
        logEvent["Properties"]["First"].ShouldBe(1);
        logEvent["Properties"]["Second"].ShouldBe(2);
        logEvent["Renderings"]["First"].Children().Count().ShouldBe(1);
        logEvent["Renderings"]["First"][0]["Format"].ShouldBe("x8");
        logEvent["Renderings"]["First"][0]["Rendering"].ShouldBe("00000001");
        logEvent["Renderings"]["Second"].Children().Count().ShouldBe(1);
        logEvent["Renderings"]["Second"][0]["Format"].ShouldBe("x8");
        logEvent["Renderings"]["Second"][0]["Rendering"].ShouldBe("00000002");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteException(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

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
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteExceptionAndProperty(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

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
        logEvent["Properties"]["Property"].ShouldBe(42);
        logEvent["Renderings"].ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SkipNastyException(bool isRenderingMessage)
    {
        // Arrange
        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

        // Act
        logger.Information(new NastyException(), "With exception");

        // Assert
        output.ToString().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WriteTraceIdAndSpanId(bool isRenderingMessage)
    {
        // Arrange
        const string activitySourceName = "Serilog.Sinks.HttpTests";
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        // at least one listener must exist in order to start activity
        using var listener = new ActivityListener();
        listener.ShouldListenTo = source => source.Name == activitySourceName;
        listener.Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded;

        ActivitySource.AddActivityListener(listener);

        using var customActivitySource = new ActivitySource(activitySourceName);
        using var activity = customActivitySource.StartActivity("WriteTraceIdAndSpanId", ActivityKind.Server);
        activity.ShouldNotBeNull();

        logger = CreateLogger(isRenderingMessage ?
            new NormalRenderedTextFormatter() :
            new NormalTextFormatter());

        // Act
        logger.Information("No properties");

        // Assert
        var logEvent = GetEvent();

        logEvent["Timestamp"].ShouldNotBeNull();
        logEvent["Level"].ShouldBe("Information");
        logEvent["TraceId"].ShouldBe(activity.TraceId.ToString());
        logEvent["SpanId"].ShouldBe(activity.SpanId.ToString());
        logEvent["MessageTemplate"].ShouldBe("No properties");
        ((string)logEvent["RenderedMessage"]).ShouldBe(isRenderingMessage ? "No properties" : null);
        logEvent["Exception"].ShouldBeNull();
        logEvent["Properties"].ShouldBeNull();
        logEvent["Renderings"].ShouldBeNull();
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
