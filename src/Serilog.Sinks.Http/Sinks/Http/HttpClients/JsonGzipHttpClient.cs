// Copyright 2015-2024 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.Http.HttpClients;

/// <summary>
/// HTTP client sending Gzip encoded JSON over the network.
/// </summary>
/// <remarks>
/// This sink will, in comparison to <seealso cref="JsonHttpClient"/>, send smaller requests
/// over the network at the expense of increased CPU and memory utilization.
/// </remarks>
/// <seealso cref="JsonHttpClient"/>
/// <seealso cref="IHttpClient"/>
public class JsonGzipHttpClient : IHttpClient
{
    private const string JsonContentType = "application/json";
    private const string GzipContentEncoding = "gzip";

    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonGzipHttpClient"/> class with
    /// fastest compression level.
    /// </summary>
    public JsonGzipHttpClient()
        : this(CompressionLevel.Fastest)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonGzipHttpClient"/> class with
    /// specified compression level.
    /// </summary>
    public JsonGzipHttpClient(CompressionLevel compressionLevel)
        : this(new HttpClient(), compressionLevel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonGzipHttpClient"/> class with
    /// specified HTTP client and compression level.
    /// </summary>
    public JsonGzipHttpClient(HttpClient httpClient, CompressionLevel compressionLevel)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        CompressionLevel = compressionLevel;
    }

    ~JsonGzipHttpClient()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets or sets the compression level.
    /// </summary>
    protected CompressionLevel CompressionLevel { get; set; }

    /// <inheritdoc />
    public virtual void Configure(IConfiguration configuration)
    {
    }

    /// <inheritdoc />
    public virtual async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        using var output = new MemoryStream();

        using (var gzipStream = new GZipStream(output, CompressionLevel, true)){
            await contentStream
                .CopyToAsync(gzipStream)
                .ConfigureAwait(false);
        }

        output.Position = 0;

        var content = new StreamContent(output);
        content.Headers.Add("Content-Type", JsonContentType);
        content.Headers.Add("Content-Encoding", GzipContentEncoding);

        var response = await httpClient
            .PostAsync(requestUri, content)
            .ConfigureAwait(false);

        return response;
    }

    /// <inheritdoc />
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