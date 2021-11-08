using System;
using System.Collections.Generic;

namespace Serilog.Sinks.HttpTests.LogServer.Services
{
    public class LogEvent
    {
        public LogEvent(
            DateTime timestamp,
            string level,
            string? messageTemplate,
            string? renderedMessage,
            Dictionary<string, object>? properties)
        {
            Timestamp = timestamp;
            Level = level;
            MessageTemplate = messageTemplate;
            RenderedMessage = renderedMessage;
            Properties = properties;
        }

        public DateTime Timestamp { get; }

        public string Level { get; }

        public string? MessageTemplate { get; }

        public string? RenderedMessage { get; }

        public Dictionary<string, object>? Properties { get; }
    }
}
