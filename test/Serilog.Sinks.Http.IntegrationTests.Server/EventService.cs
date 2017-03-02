using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server
{
	public class EventService : IEventService
	{
		private readonly ConcurrentQueue<Event> cache;

		public EventService()
		{
			cache = new ConcurrentQueue<Event>();
		}

		public void Add(IEnumerable<Event> events)
		{
			foreach (var @event in events)
			{
				cache.Enqueue(@event);
			}
		}

		public IEnumerable<Event> Get()
		{
			return cache.ToArray();
		}
	}
}
