using System.Collections.Generic;

namespace Serilog.Sinks.Http.LogServer.Controllers.Dto
{
	public class EventBatchRequestDto
	{
		public IEnumerable<EventDto> Events { get; set; }
	}
}
