using System;

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
					requestUri: "api/batches",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
					httpClient: HttpClient)
				.CreateLogger();
		}
	}
}
