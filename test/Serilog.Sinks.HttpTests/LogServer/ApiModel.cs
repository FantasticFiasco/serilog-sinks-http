using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Serilog.Sinks.Http.LogServer;
using Serilog.Sinks.Http.LogServer.Controllers;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;
using Xunit;
using Xunit.Sdk;

namespace Serilog.LogServer
{
	public class ApiModel
	{
		private readonly HttpClient client;
		private readonly Policy retryPolicy;

		public ApiModel(HttpClient client)
		{
			this.client = client;
			retryPolicy = Policy
				.Handle<XunitException>()
				.WaitAndRetryAsync(10, retryCount => TimeSpan.FromSeconds(1));
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

		public async Task<Event[]> GetAsync()
		{
			var response = await client.GetAsync("/api/events");

			Assert.True(response.IsSuccessStatusCode);

			var content = await response.Content.ReadAsStringAsync();
			var events = JsonConvert.DeserializeObject<IEnumerable<EventDto>>(content);

			return events
                .Select(PayloadConvert.FromDto)
                .ToArray();
		}

		public Task<Event[]> WaitAndGetAsync(int expectedEventCount)
		{
			return retryPolicy.ExecuteAsync(
				async () =>
				{
					var actual = await GetAsync();
					int actualCount = actual.Length;

					if (actualCount > expectedEventCount)
					{
						throw new Exception($"Expected {expectedEventCount} but got {actualCount}");
					}

					if (actualCount != expectedEventCount)
					{
						throw new XunitException();
					}

					return actual;
				});
		}
	}
}
