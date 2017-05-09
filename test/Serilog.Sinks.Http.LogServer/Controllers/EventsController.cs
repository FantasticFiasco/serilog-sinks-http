using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;

namespace Serilog.Sinks.Http.LogServer.Controllers
{
	[Route("api/[controller]")]
	public class EventsController
	{
		private readonly IEventService eventService;

		public EventsController(IEventService eventService)
		{
			this.eventService = eventService;
		}

		// POST /api/events
		[HttpPost]
		public void Post([FromBody] EventBatchRequestDto batch)
		{
			var events = batch.Events.Select(FromDto);
			eventService.Add(events);
		}

	    private static Event FromDto(EventDto @event)
	    {
	        return new Event(
	            @event.Timestamp,
	            @event.Level,
	            @event.MessageTemplate,
	            @event.Properties,
	            @event.RenderedMessage,
	            @event.Exception);
	    }
    }
}
