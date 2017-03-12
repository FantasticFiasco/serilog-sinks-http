using System;
using Serilog.Sinks.Http;

namespace Serilog
{
	public class HttpSinkTest : SinkFixture
	{
		public HttpSinkTest()
		{
			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.Http(
					"api/batches",
					new Options
					{
						BatchPostingLimit = 100,
						Period = TimeSpan.FromMilliseconds(1)
					},
					httpClient: HttpClient)
				.CreateLogger();
		}
	}
}
