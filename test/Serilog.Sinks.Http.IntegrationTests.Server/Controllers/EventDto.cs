using System;
using Newtonsoft.Json;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	public class EventDto
	{
		[JsonProperty(PropertyName = "@t")]
		public DateTime Timestamp { get; set; }

		[JsonProperty(PropertyName = "@l")]
		public string Level { get; set; }

		[JsonProperty(PropertyName = "@mt")]
		public string MessageTemplate { get; set; }

		[JsonProperty(PropertyName = "@x")]
		public string Exception { get; set; }
	}
}