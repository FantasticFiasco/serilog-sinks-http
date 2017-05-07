using System;
using Serilog.Sinks.Http;

namespace Serilog
{
	public class CodeConfigurationHttpTest : SinkFixture
	{
		public CodeConfigurationHttpTest()
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
