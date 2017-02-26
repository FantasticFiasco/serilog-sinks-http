using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Serilog.Sinks.Http.IntegrationTests.Server;
using Serilog.Sinks.Http.IntegrationTests.Server.Controllers;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class Temp
    {
		private readonly TestServer _server;
		private readonly HttpClient _client;

		public Temp()
		{
			// Arrange
			_server = new TestServer(new WebHostBuilder()
				.UseStartup<Startup>());
			_client = _server.CreateClient();
		}

		[Fact]
		public async Task ReturnHelloWorld()
		{
			// Act
			var dto = new LogDto
			{
				Events = new string[]
				{
					"Kalle",
					"Olle"
				}
			};

			var response = await _client.PostAsync(
				"/api/logs",
				new StringContent(JsonConvert.SerializeObject(dto),
				Encoding.UTF8,
				"application/json"));

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}
	}
}
