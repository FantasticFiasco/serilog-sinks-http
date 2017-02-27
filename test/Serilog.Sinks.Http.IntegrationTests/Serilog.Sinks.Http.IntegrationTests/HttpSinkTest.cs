using System;
using System.Threading.Tasks;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class HttpSinkTest : TestServerFixture
    {
	    private readonly ILogger logger;
		
		public HttpSinkTest()
		{
			ClearBufferFiles();

			logger = new LoggerConfiguration()
				.WriteTo
				.Http(
					"api/batches",
					BufferBaseFilename,
					batchPostingLimit: 100,
					period: TimeSpan.FromMilliseconds(1),
					httpClient: new TestServerHttpClient(Server.CreateClient()))
				.CreateLogger();
		}

	    [Fact]
	    public async Task Error()
	    {
		    // Act
			logger.Error("Some message");

			// Assert
		    Assert.True(await Api.WaitForEventCount(1));
	    }
	}
}
