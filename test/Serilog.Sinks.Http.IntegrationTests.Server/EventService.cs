using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server
{
	public class EventService : IEventService
	{
		private readonly List<Event> cache;

		public EventService()
		{
			cache = new List<Event>();
		}

		public void Add(IEnumerable<Event> events)
		{
			cache.AddRange(events);
		}

		public IEnumerable<Event> Get()
		{
			return cache;
		}
	}
}
