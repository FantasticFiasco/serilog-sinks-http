using System.Net.Http;
using System.Threading.Tasks;

namespace Serilog.Sinks.Http.IntegrationTests
{
	public class TestServerHttpClient : IHttpClient
	{
		private readonly HttpClient client;

		public TestServerHttpClient(HttpClient client)
		{
			this.client = client;
		}

		public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
			return client.PostAsync(requestUri, content);
		}

		public void Dispose()
		{
			client.Dispose();
		}
	}
}
