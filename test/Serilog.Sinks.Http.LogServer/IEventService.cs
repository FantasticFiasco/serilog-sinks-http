using System.Collections.Generic;

namespace Serilog.Sinks.Http.LogServer
{
	public interface IEventService
	{
		void Add(IEnumerable<Event> events);

		IEnumerable<Event> Get();
	}
}
