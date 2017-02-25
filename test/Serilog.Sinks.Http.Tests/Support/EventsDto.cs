using System.Collections.Generic;

namespace Serilog.Sinks.Http.Support
{
    public class EventsDto
    {
	    public IEnumerable<object> events { get; set; }
    }
}
