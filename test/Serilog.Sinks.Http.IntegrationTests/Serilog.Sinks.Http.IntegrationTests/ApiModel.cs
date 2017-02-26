using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Sinks.Http.IntegrationTests.Server;
using Serilog.Sinks.Http.IntegrationTests.Server.Controllers;
using Xunit;

namespace Serilog.Sinks.Http.IntegrationTests
{
    public class ApiModel
    {
	    private readonly HttpClient client;

	    public ApiModel(HttpClient client)
	    {
		    this.client = client;
	    }

	    public async Task AddAsync(IEnumerable<Event> events)
	    {
		    var request = new EventBatchRequestDto
		    {
			    Events = events.Select(@event => new EventDto { Payload = @event.Payload })
		    };

			var response = await client.PostAsync(
				"/api/events/batch",
				new StringContent(JsonConvert.SerializeObject(request),
				Encoding.UTF8,
				"application/json"));

		    Assert.True(response.IsSuccessStatusCode);
	    }

	    public async Task<IEnumerable<Event>> GetAsync()
	    {
			var response = await client.GetAsync("/api/events");

			Assert.True(response.IsSuccessStatusCode);

		    var content = await response.Content.ReadAsStringAsync();
			var events = JsonConvert.DeserializeObject<IEnumerable<EventDto>>(content);

		    return events.Select(@event => new Event(@event.Payload));
	    }
	}
}
