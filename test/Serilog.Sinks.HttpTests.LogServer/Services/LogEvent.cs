﻿namespace Serilog.Sinks.HttpTests.LogServer.Services;

public record LogEvent(
    DateTime Timestamp,
    string Level,
    string? MessageTemplate,
    string? RenderedMessage,
    Dictionary<string, object>? Properties,
    Dictionary<string, LogEventRendering[]>? Renderings,
    string? Exception)
{
}

public record LogEventRendering(
    string Format,
    string Rendering);
