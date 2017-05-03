using System;
using System.IO;
using System.Linq;
using Serilog.Sinks.Http;
using IOFile = System.IO.File;

namespace Serilog
{
	public class DurableHttpSinkTest : SinkFixture
	{
		public DurableHttpSinkTest()
		{
			ClearBufferFiles();

			Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.DurableHttp(
					"api/batches",
					new DurableOptions
					{
						BatchPostingLimit = 100,
						Period = TimeSpan.FromMilliseconds(1)
					},
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
