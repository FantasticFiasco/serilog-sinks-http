using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Serilog.Sinks.Http.IntegrationTests.Server;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class Temp
    {
		private readonly TestServer server;
	    private readonly ApiModel api;

		public Temp()
		{
			server = new TestServer(new WebHostBuilder()
				.UseStartup<Startup>());

			api = new ApiModel(server.CreateClient());
		}

		[Fact]
		public async Task ReturnHelloWorld()
		{
			await api.AddAsync(new[]
			{
				new Event("Kalle" ),
				new Event("Olle")
			});
			
			var events = await api.GetAsync();
			
			Assert.Equal(2, events.Count());
		}

		[Fact]
		public async Task ReturnHelloWorld2()
		{
			var events = await api.GetAsync();

			Assert.Equal(0, events.Count());
		}
	}
}
