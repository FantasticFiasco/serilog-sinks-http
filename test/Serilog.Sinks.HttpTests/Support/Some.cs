using System;
using System.IO;
using Serilog.Events;
using Serilog.Sinks.Http.TextFormatters;
using Xunit.Sdk;

namespace Serilog.Support;

public static class Some
{
    public static LogEvent LogEvent(
        string messageTemplate,
        params object[] propertyValues)
    {
        return LogEvent(null, messageTemplate, propertyValues);
    }

    public static string SerializedLogEvent(
        string messageTemplate,
        params object[] propertyValues)
    {
        return Serialize(LogEvent(messageTemplate, propertyValues));
    }

    public static LogEvent LogEvent(
        Exception exception,
        string messageTemplate,
        params object[] propertyValues)
    {
        return LogEvent(LogEventLevel.Information, exception, messageTemplate, propertyValues);
    }

    public static string SerializedLogEvent(
        Exception exception,
        string messageTemplate,
        params object[] propertyValues)
    {
        return Serialize(LogEvent(exception, messageTemplate, propertyValues));
    }

    public static LogEvent LogEvent(
        LogEventLevel level,
        Exception exception,
        string messageTemplate,
        params object[] propertyValues)
    {
        var log = new LoggerConfiguration().CreateLogger();

        if (!log.BindMessageTemplate(messageTemplate, propertyValues, out var template, out var properties))
        {
            throw new XunitException("Template could not be bound");
        }

        return new LogEvent(DateTimeOffset.Now, level, exception, template, properties);
    }

    public static string SerializedLogEvent(
        LogEventLevel level,
        Exception exception,
        string messageTemplate,
        params object[] propertyValues)
    {
        return Serialize(LogEvent(level, exception, messageTemplate, propertyValues));
    }

    public static LogEvent DebugEvent()
    {
        return LogEvent(LogEventLevel.Debug, null, "Debug event");
    }

    public static string SerializedDebugEvent()
    {
        return Serialize(DebugEvent());
    }

    public static LogEvent InformationEvent()
    {
        return LogEvent(LogEventLevel.Information, null, "Information event");
    }

    public static string SerializedInformationEvent()
    {
        return Serialize(InformationEvent());
    }

    public static LogEvent ErrorEvent()
    {
        return LogEvent(LogEventLevel.Error, null, "Error event");
    }

    public static string SerializedErrorEvent()
    {
        return Serialize(ErrorEvent());
    }

    private static string Serialize(LogEvent logEvent)
    {
        var writer = new StringWriter();
        var formatter = new NormalRenderedTextFormatter();
        formatter.Format(logEvent, writer);
        return writer.ToString();
    }
}