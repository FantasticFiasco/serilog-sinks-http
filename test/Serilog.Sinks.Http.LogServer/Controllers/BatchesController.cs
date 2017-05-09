﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;

namespace Serilog.Sinks.Http.LogServer.Controllers
{
	[Route("api/[controller]")]
	public class BatchesController
	{
		private readonly IEventService eventService;

		public BatchesController(IEventService eventService)
		{
			this.eventService = eventService;
		}

		// POST /api/batches
		[HttpPost]
		public void Post([FromBody] EventBatchRequestDto batch)
		{
			var events = batch.Events.Select(PayloadConvert.FromDto);
			eventService.Add(events);
		}
	}
}
