using System.Collections.Generic;
using System.Linq;
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
			var request = new EventBatchRequestDto
			{
				Events = new[]
				{
					new EventDto { Payload = "Kalle" },
					new EventDto { Payload = "Olle" }
				}
			};

			var response = await _client.PostAsync(
				"/api/events/batch",
				new StringContent(JsonConvert.SerializeObject(request),
				Encoding.UTF8,
				"application/json"));

			Assert.Equal(HttpStatusCode.OK, response.StatusCode);

			response = await _client.GetAsync("api/events");

			var events = JsonConvert.DeserializeObject<IEnumerable<EventDto>>(await response.Content.ReadAsStringAsync());

			Assert.Equal(2, events.Count());
		}
	}
}
