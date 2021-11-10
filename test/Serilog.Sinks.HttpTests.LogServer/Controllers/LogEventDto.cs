using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers;

public record LogEventDto(
    DateTime Timestamp,
    string Level,
    string? MessageTemplate,
    string? RenderedMessage,
    Dictionary<string, object>? Properties)
{
    public LogEvent ToLogEvent()
    {
        return new LogEvent(Timestamp, Level, MessageTemplate, RenderedMessage, Properties);
    }
}
