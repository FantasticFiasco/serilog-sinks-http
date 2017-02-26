using Microsoft.AspNetCore.Mvc;

namespace Serilog.Sinks.Http.IntegrationTests.Server.Controllers
{
	[Route("api/[controller]")]
	public class LogsController : Controller
    {
		// POST api/values
		[HttpPost]
		public void Post([FromBody]LogDto value)
		{
		}
	}
}
