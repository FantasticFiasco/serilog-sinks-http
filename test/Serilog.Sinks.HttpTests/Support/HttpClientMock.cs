using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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

        public int BatchCount
        {
            get { return batches.Count; }
        }

        public string[] LogEvents
        {
            get
            {
                return batches
                    .SelectMany(batch => batch.Events)
                    .Select(logEvent => logEvent.RenderedMessage)
                    .ToArray();
            }
        }

        public IConfiguration Configuration { get; private set; }

        public async Task WaitAsync(int expectedLogEventCount)
        {
            // 10 000 iterations, each waiting at least 1ms, means that a test has 10s to pass
            for (var i = 0; i < 10_000; i++)
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

        public void SimulateNetworkFailure()
        {
            simulateNetworkFailure = true;
        }

        public void Configure(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            if (simulateNetworkFailure)
            {
                simulateNetworkFailure = false;

                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            // Make sure content stream doesn't contain BOM
            var head = new byte[3];
            await contentStream.ReadAsync(head, 0, 3);
            if (head.SequenceEqual(System.Text.Encoding.UTF8.GetPreamble()))
            {
                throw new XunitException("Content stream contains UTF8 BOM");
            }

            contentStream.Position = 0;

            DefaultBatch batch;

            try
            {
                using var reader = new StreamReader(contentStream);
                batch = JsonConvert.DeserializeObject<DefaultBatch>(await reader.ReadToEndAsync());
            }
            catch (Exception)
            {
                throw new XunitException($"{nameof(HttpClientMock)} assume log events are formatted using {nameof(NormalRenderedTextFormatter)}, and batches are formatted using {nameof(DefaultBatchFormatter)}");
            }

            batches.Enqueue(batch);

            return new HttpResponseMessage
            {
                Content = new StreamContent(contentStream)
            };
        }

        public void Dispose()
        {
        }
    }
}
