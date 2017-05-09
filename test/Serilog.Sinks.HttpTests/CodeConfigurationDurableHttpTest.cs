using System;
using System.IO;
using System.Linq;
using Serilog.LogServer;
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
					requestUri: "api/events",
                    batchPostingLimit: 100,
                    period: TimeSpan.FromMilliseconds(1),
					httpClient: new TestServerHttpClient())
				.CreateLogger();

		    TestServerHttpClient.Instance.Client = Server.CreateClient();
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
