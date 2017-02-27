using System;
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
			    Events = events.Select(PayloadConvert.ToDto)
		    };

			var response = await client.PostAsync(
				"/api/batches",
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

		    return events.Select(PayloadConvert.FromDto);
	    }

	    public async Task<bool> WaitForEventCount(int expected)
	    {
		    var retryCount = 10;
		    var period = TimeSpan.FromSeconds(1);

		    for (int currentAttempt = 0; currentAttempt < retryCount; currentAttempt++)
		    {
			    var actual = (await GetAsync()).Count();

				Assert.True(actual <= expected);

			    if (actual == expected)
			    {
				    return true;
			    }

			    await Task.Delay(period);
		    }

		    return false;
	    }
    }
}
