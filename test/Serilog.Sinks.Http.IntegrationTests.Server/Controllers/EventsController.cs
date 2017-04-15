using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.Http.Shared.Dto;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	[Route("api/[controller]")]
	public class EventsController : Controller
	{
		private readonly IEventService eventService;

		public EventsController(IEventService eventService)
		{
			this.eventService = eventService;
		}

		// GET /api/events
		[HttpGet]
		public IEnumerable<EventDto> Get()
		{
			return eventService.Get()
				.Select(PayloadConvert.ToDto);
		}
	}
}
