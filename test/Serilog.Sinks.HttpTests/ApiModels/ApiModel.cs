using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Serilog.Sinks.Http.IntegrationTests.Server;
using Serilog.Sinks.Http.IntegrationTests.Server.Controllers;
using Serilog.Sinks.Http.IntegrationTests.Server.Controllers.Dto;
using Xunit;
using Xunit.Sdk;

namespace Serilog.ApiModels
{
	public class ApiModel
	{
		private readonly HttpClient client;
		private readonly Policy waitPolicy;

		public ApiModel(HttpClient client)
		{
			this.client = client;
			waitPolicy = Policy
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

		public async Task<IEnumerable<Event>> GetAsync()
		{
			var response = await client.GetAsync("/api/events");

			Assert.True(response.IsSuccessStatusCode);

			var content = await response.Content.ReadAsStringAsync();
			var events = JsonConvert.DeserializeObject<IEnumerable<EventDto>>(content);

			return events.Select(PayloadConvert.FromDto);
		}

		public Task<IEnumerable<Event>> WaitAndGetAsync(int expectedEventCount)
		{
			return waitPolicy.ExecuteAsync(
				async () =>
				{
					var actual = await GetAsync();
					int actualCount = actual.Count();

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
