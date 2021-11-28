using System.Text.Json.Serialization;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers;

public class LogEventDto
{
    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("Level")]
    public string Level { get; set; } = "Information";

    [JsonPropertyName("MessageTemplate")]
    public string? MessageTemplate { get; set; }

    [JsonPropertyName("RenderedMessage")]
    public string? RenderedMessage { get; set; }

    [JsonPropertyName("Properties")]
    public Dictionary<string, object>? Properties { get; set; }

    [JsonPropertyName("Renderings")]
    public Dictionary<string, LogEventRendering[]>? Renderings { get; set; }

    [JsonPropertyName("Exception")]
    public string? Exception { get; set; }

    public static LogEventDto From(LogEvent logEvent)
    {
        return new LogEventDto
        {
            Timestamp = logEvent.Timestamp,
            Level = logEvent.Level,
            MessageTemplate = logEvent.MessageTemplate,
            RenderedMessage = logEvent.RenderedMessage,
            Properties = logEvent.Properties,
            Renderings = logEvent.Renderings,
            Exception = logEvent.Exception,
        };
    }

    public LogEvent ToLogEvent()
    {
        return new LogEvent(
            Timestamp,
            Level,
            MessageTemplate,
            RenderedMessage,
            Properties,
            Renderings,
            Exception);
    }
}
