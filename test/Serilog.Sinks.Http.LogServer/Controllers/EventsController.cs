using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;

namespace Serilog.Sinks.Http.LogServer.Controllers
{
	[Route("api/[controller]")]
	public class EventsController
	{
		private readonly EventService eventService;
	    private readonly NetworkService networkService;

		public EventsController(EventService eventService, NetworkService networkService)
		{
			this.eventService = eventService;
		    this.networkService = networkService;
		}

		// POST /api/events
		[HttpPost]
		public IActionResult Post([FromBody] EventBatchRequestDto batch)
		{
		    if (networkService.IsSimulatingNetworkFailure)
		    {
		        networkService.IsSimulatingNetworkFailure = false;
		        return new NotFoundResult();
		    }
            
			var events = batch.Events.Select(FromDto);
			eventService.Add(events);

		    return new OkResult();
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
