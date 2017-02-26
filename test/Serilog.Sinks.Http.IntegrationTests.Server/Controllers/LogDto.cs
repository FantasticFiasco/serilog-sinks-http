using System.Collections.Generic;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
    public class LogDto
    {
	    public IEnumerable<string> Events { get; set; }
    }
}
