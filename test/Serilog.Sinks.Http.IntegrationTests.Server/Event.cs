using System;

namespace Serilog.Sinks.Http.IntegrationTests.Server
{
    public class Event
    {
	    public Event(DateTime timestamp, string level, string messageTemplate, string renderedMessage)
	    {
		    Timestamp = timestamp;
		    Level = level;
		    MessageTemplate = messageTemplate;
		    RenderedMessage = renderedMessage;
	    }

		public DateTime Timestamp { get; }

		public string Level { get; }

		public string MessageTemplate { get; }

		public string RenderedMessage { get; }
	}
}
