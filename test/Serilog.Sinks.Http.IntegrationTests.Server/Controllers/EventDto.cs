using System;
using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	public class EventDto
	{
		public DateTime Timestamp { get; set; }

		public string Level { get; set; }

		public string MessageTemplate { get; set; }

		public Dictionary<string, string> Properties { get; set; }

		public string Exception { get; set; }
	}
}