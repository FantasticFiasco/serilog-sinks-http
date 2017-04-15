using System.Collections.Generic;

namespace Serilog.Sinks.Http.Shared.Dto
{
	public class EventBatchRequestDto
	{
		public IEnumerable<EventDto> Events { get; set; }
	}
}
