using System;
using Serilog.Events;
using Xunit.Sdk;

namespace Serilog.Support
{
    public static class Some
    {
        public static LogEvent LogEvent(
            string messageTemplate,
            params object[] propertyValues)
        {
            return LogEvent(null, messageTemplate, propertyValues);
        }

        public static LogEvent LogEvent(
            Exception exception,
            string messageTemplate,
            params object[] propertyValues)
        {
            return LogEvent(LogEventLevel.Information, exception, messageTemplate, propertyValues);
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

        public static LogEvent DebugEvent()
        {
            return LogEvent(LogEventLevel.Debug, null, "Debug event");
        }

        public static LogEvent InformationEvent()
        {
            return LogEvent(LogEventLevel.Information, null, "Information event");
        }

        public static LogEvent ErrorEvent()
        {
            return LogEvent(LogEventLevel.Error, null, "Error event");
        }
    }
}
