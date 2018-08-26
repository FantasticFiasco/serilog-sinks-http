using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog.Sinks.Http;

namespace Serilog.Support
{
    public class InMemoryHttpClient : IHttpClient
    {
        private readonly List<HttpContent> events;

        public InMemoryHttpClient() =>
            events = new List<HttpContent>();

        public HttpContent[] Events =>
            events.ToArray();

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            events.Add(content);
            return Task.FromResult(new HttpResponseMessage());
        }
            
        public void Dispose()
        {
        }
    }
}
