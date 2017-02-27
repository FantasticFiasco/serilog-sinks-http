using System;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class HttpSinkTest : TestServerFixture, IDisposable
    {
	    private readonly Logger logger;
		
		public HttpSinkTest()
		{
			ClearBufferFiles();

			logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo
				.Http(
					"api/batches",
					BufferBaseFilename,
					batchPostingLimit: 100,
					period: TimeSpan.FromMilliseconds(1),
					httpClient: new TestServerHttpClient(Server.CreateClient()))
				.CreateLogger();
		}

		[Theory]
		[InlineData(LogEventLevel.Verbose)]
		[InlineData(LogEventLevel.Debug)]
		[InlineData(LogEventLevel.Information)]
		[InlineData(LogEventLevel.Warning)]
		[InlineData(LogEventLevel.Error)]
		[InlineData(LogEventLevel.Fatal)]
		public async Task Write(LogEventLevel level)
		{
			// Act
			logger.Write(level, "Some message");

			// Assert
			await Api.WaitForEventCountAsync(1);
		}

	    void IDisposable.Dispose()
	    {
		    logger?.Dispose();
	    }
    }
}
