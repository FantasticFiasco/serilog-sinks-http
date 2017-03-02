using System;

namespace Serilog.Sinks.Http.IntegrationTests.Server
{
	public class Event
	{
		public Event(
			DateTime timestamp,
			string level,
			string messageTemplate,
			string exception)
		{
			Timestamp = timestamp;
			Level = level;
			MessageTemplate = messageTemplate;
			Exception = exception;
		}

		public DateTime Timestamp { get; }

		public string Level { get; }

		public string MessageTemplate { get; }

		public string Exception { get; set; }
	}
}
