using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Serilog
{
	public class TestServerHttpClient : IHttpClient
	{
		private readonly HttpClient client;
		private readonly object syncRoot;

		private bool simulateNetworkFailure;

		public TestServerHttpClient(HttpClient client)
		{
			this.client = client;

			syncRoot = new object();
		}

		public int NumberOfPosts { get; private set; }

		public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
			// Totally abusing the asynchronous nature of tasks, but this is tests, so I'll let it
			// slip
			lock (syncRoot)
			{
				NumberOfPosts++;

				if (simulateNetworkFailure)
				{
					simulateNetworkFailure = false;
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
				}

				return client.PostAsync(requestUri, content);
			}
		}

		public void SimulateNetworkFailure()
		{
			lock (syncRoot)
			{
				simulateNetworkFailure = true;
			}
		}

		public void Dispose()
		{
			client.Dispose();
		}
	}
}
