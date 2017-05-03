using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers.Dto
{
	public class EventBatchRequestDto
	{
		public IEnumerable<EventDto> Events { get; set; }
	}
}
