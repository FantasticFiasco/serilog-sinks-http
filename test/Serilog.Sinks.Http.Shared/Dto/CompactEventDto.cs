using System;
using Newtonsoft.Json;

namespace Serilog.Sinks.Http.Shared.Dto
{
	public class CompactEventDto
	{
		[JsonProperty("@t")]
		public DateTime Timestamp { get; set; }

		[JsonProperty("@l")]
		public string Level { get; set; }

		[JsonProperty("@mt")]
		public string MessageTemplate { get; set; }

		[JsonProperty("@m")]
		public string RenderedMessage { get; set; }

		[JsonProperty("@x")]
		public string Exception { get; set; }

		[JsonProperty("@r")]
		public string[] Renderings { get; set; }
	}
}
