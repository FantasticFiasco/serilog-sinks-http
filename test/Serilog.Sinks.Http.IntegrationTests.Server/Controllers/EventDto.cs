using System;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	public class EventDto
	{
		public DateTime Timestamp { get; set; }

		public string Level { get; set; }

		public string MessageTemplate { get; set; }

		public string RenderedMessage { get; set; }
	}
}