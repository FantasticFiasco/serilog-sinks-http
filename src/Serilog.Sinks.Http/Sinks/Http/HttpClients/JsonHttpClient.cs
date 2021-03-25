// Copyright 2015-2020 Serilog Contributors
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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Serilog.Sinks.Http.HttpClients
{
    // TODO: Document
    public class JsonHttpClient : IHttpClient
    {
        private const string JsonContentType = "application/json";

        private readonly HttpClient httpClient;

        public JsonHttpClient() => httpClient = new HttpClient();

        ~JsonHttpClient() => Dispose(false);

        public virtual void Configure(IConfiguration configuration)
        {
        }

        public virtual async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            using var content = new StreamContent(contentStream);
            content.Headers.Add("Content-Type", JsonContentType);

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
