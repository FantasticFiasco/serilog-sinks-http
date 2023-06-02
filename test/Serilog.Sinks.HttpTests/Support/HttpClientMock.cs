using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.Http;

namespace Serilog.Support;

class HttpClientMock : IHttpClient
{
    public IConfiguration Configuration { get; private set; }

    public void Configure(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void Dispose()
    {
    }

    public Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        throw new NotImplementedException();
    }
}