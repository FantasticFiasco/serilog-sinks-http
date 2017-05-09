using System;
using System.Collections.Generic;

namespace Serilog.Sinks.Http.LogServer
{
    public class Event
    {
        public Event(
            DateTime timestamp,
            string level,
            string messageTemplate,
            Dictionary<string, string> properties,
            string renderedMessage,
            string exception)
        {
            Timestamp = timestamp;
            Level = level;
            MessageTemplate = messageTemplate;
            Properties = properties;
            RenderedMessage = renderedMessage;
            Exception = exception;
        }

        public DateTime Timestamp { get; }

        public string Level { get; }

        public string MessageTemplate { get; }

        public string RenderedMessage { get; set; }

        public Dictionary<string, string> Properties { get; }

        public string Exception { get; set; }
    }
}
