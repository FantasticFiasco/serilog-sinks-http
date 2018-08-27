using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Sinks.Http;
using Xunit.Sdk;

namespace Serilog.Support
{
    public class HttpClientMock : IHttpClient
    {
        private readonly ConcurrentQueue<Batch> batches;

        private bool simulateNetworkFailure;

        public HttpClientMock()
        {
            batches = new ConcurrentQueue<Batch>();

            Instance = this;
        }

        public static HttpClientMock Instance { get; private set; }

        public int BatchCount =>
            batches.Count;

        public string[] LogEvents =>
            batches.SelectMany(batch => batch.LogEvents).ToArray();

        public async Task WaitAsync(int expectedLogEventCount)
        {
            // 10 000 iterations, each waiting at least 1ms, means that a test has 10s to pass
            for (int i = 0; i < 10_000; i++)
            {
                if (LogEvents.Length == expectedLogEventCount)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1));
            }

            if (LogEvents.Length != expectedLogEventCount)
            {
                throw new XunitException($"Expected {expectedLogEventCount} log event(s) but got {LogEvents.Length}");
            }
        }

        public void SimulateNetworkFailure() =>
            simulateNetworkFailure = true;

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            if (simulateNetworkFailure)
            {
                simulateNetworkFailure = false;

                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            // Assume log events are formatted using RenderedMessageTextFormatter, and batches are
            // formatted using ArrayBatchFormatter
            var logEvents = JsonConvert.DeserializeObject<string[]>(await content.ReadAsStringAsync());

            var batch = new Batch
            {
                LogEvents = logEvents
            };

            batches.Enqueue(batch);

            return new HttpResponseMessage
            {
                Content = content
            };
        }
            
        public void Dispose()
        {
        }

        private class Batch
        {
            public string[] LogEvents { get; set; }
        }
    }
}
