using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	public class EventsController : Controller
	{
		private readonly IEventService eventService;

		public EventsController(IEventService eventService)
		{
			this.eventService = eventService;
		}

		// POST /api/events/batch
		[HttpPost]
		[Route("api/events/batch")]
		public void Post([FromBody] EventBatchRequestDto batch)
		{
			var events = batch.Events.Select(@event => new Event(@event.Payload));
			eventService.Add(events);
		}

		// GET /api/events
		[HttpGet]
		[Route("api/events")]
	    public IEnumerable<EventDto> Get()
	    {
		    return eventService.Get()
				.Select(@event => new EventDto { Payload = @event.Payload });
	    }
	}
}
