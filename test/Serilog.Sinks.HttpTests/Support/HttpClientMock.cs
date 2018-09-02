using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog.Sinks.Http;
using Serilog.Sinks.Http.BatchFormatters;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support.BatchFormatters;
using Xunit.Sdk;

namespace Serilog.Support
{
    public class HttpClientMock : IHttpClient
    {
        private readonly ConcurrentQueue<DefaultBatch> batches;

        private bool simulateNetworkFailure;

        public HttpClientMock()
        {
            batches = new ConcurrentQueue<DefaultBatch>();

            Instance = this;
        }

        public static HttpClientMock Instance { get; private set; }

        public int BatchCount =>
            batches.Count;

        public string[] LogEvents =>
            batches
                .SelectMany(batch => batch.Events)
                .Select(logEvent => logEvent.RenderedMessage)
                .ToArray();

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

            DefaultBatch batch;

            try
            {
                batch = JsonConvert.DeserializeObject<DefaultBatch>(await content.ReadAsStringAsync());
            }
            catch (Exception)
            {
                throw new XunitException($"{nameof(HttpClientMock)} assume log events are formatted using {nameof(NormalTextFormatter)}, and batches are formatted using {nameof(DefaultBatchFormatter)}");
            }

            batches.Enqueue(batch);

            return new HttpResponseMessage
            {
                Content = content
            };
        }
            
        public void Dispose()
        {
        }
    }
}
