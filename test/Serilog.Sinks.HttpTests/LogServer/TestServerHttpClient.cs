using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Serilog.LogServer
{
	public class TestServerHttpClient : IHttpClient
	{
		private readonly object syncRoot;

		private bool simulateNetworkFailure;

	    public TestServerHttpClient()
	    {
	        syncRoot = new object();
            Instance = this;
	    }

        public static TestServerHttpClient Instance { get; private set; }

		public HttpClient Client { get; set; }

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

				return Client.PostAsync(requestUri, content);
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
            Client?.Dispose();
            Client = null;
        }
	}
}
