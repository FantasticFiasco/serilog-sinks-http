using System;
using System.Collections.Generic;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    public class LogEventDto
    {
        public DateTime Timestamp { get; set; }

        public string Level { get; set; } = "Information";

        public string? MessageTemplate { get; set; }

        public string? RenderedMessage { get; set; }

        public Dictionary<string, object>? Properties { get; set; }

        public LogEvent ToLogEvent()
        {
            return new LogEvent(Timestamp, Level, MessageTemplate, RenderedMessage, Properties);
        }

        public static LogEventDto From(LogEvent logEvent)
        {
            return new LogEventDto
            {
                Timestamp = logEvent.Timestamp,
                Level = logEvent.Level,
                MessageTemplate = logEvent.MessageTemplate,
                RenderedMessage = logEvent.RenderedMessage,
                Properties = logEvent.Properties
            };
        }
    }
}
