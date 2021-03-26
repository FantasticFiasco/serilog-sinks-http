using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.Http.HttpClients
{
    // TODO: Document
    public class JsonGzipHttpClient : IHttpClient
    {
        private const string JsonContentType = "application/json";
        private const string GzipContentEncoding = "gzip";

        private readonly HttpClient httpClient;

        public JsonGzipHttpClient()
            : this(CompressionLevel.Fastest)
        {
        }

        public JsonGzipHttpClient(CompressionLevel compressionLevel)
        {
            httpClient = new HttpClient();

            CompressionLevel = compressionLevel;
        }

        ~JsonGzipHttpClient() => Dispose(false);

        protected CompressionLevel CompressionLevel { get; set; }

        public virtual void Configure(IConfiguration configuration)
        {
        }

        // TODO: Implement with lower memory footprint
        public virtual async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            using var output = new MemoryStream();
            using var gzipStream = new GZipStream(output, CompressionLevel);
            
            await contentStream
                .CopyToAsync(gzipStream)
                .ConfigureAwait(false);

            output.Position = 0;

            var content = new StreamContent(output);
            content.Headers.Add("Content-Type", JsonContentType);
            content.Headers.Add("Content-Encoding", GzipContentEncoding);

            var response = await httpClient
                .PostAsync(requestUri, content)
                .ConfigureAwait(false);

            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }
        }
    }
}
