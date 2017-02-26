using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server
{
    public interface IEventService
    {
	    void Add(IEnumerable<Event> events);

	    IEnumerable<Event> Get();
    }
}
