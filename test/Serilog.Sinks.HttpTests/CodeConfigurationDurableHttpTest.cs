using System;
using System.IO;
using System.Linq;
using IOFile = System.IO.File;

namespace Serilog
{
	public class CodeConfigurationDurableHttpTest : SinkFixture
	{
		public CodeConfigurationDurableHttpTest()
		{
			ClearBufferFiles();

			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.DurableHttp(
					requestUri: "api/batches",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
					httpClient: HttpClient)
				.CreateLogger();
		}

		private static void ClearBufferFiles()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Buffer*")
				.ToArray();

			foreach (var file in files)
			{
				IOFile.Delete(file);
			}
		}
	}
}
