using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Serilog.LogServer
{
	public class TestServerHttpClient : IHttpClient
	{
	    private int numberOfPosts;

	    public TestServerHttpClient()
	    {
	        Instance = this;
	    }
        
        public static TestServerHttpClient Instance { get; private set; }

		public HttpClient Client { get; set; }

	    public int NumberOfPosts => numberOfPosts;

	    public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
		{
		    Interlocked.Increment(ref numberOfPosts);

            return Client.PostAsync(requestUri, content);
		}

		public void Dispose()
		{
            Client?.Dispose();
        }
	}
}
