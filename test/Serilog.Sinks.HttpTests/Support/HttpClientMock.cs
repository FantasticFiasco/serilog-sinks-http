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
        private readonly ConcurrentQueue<Exception> exceptions;

        private bool simulateNetworkFailure;

        public HttpClientMock()
        {
            batches = new ConcurrentQueue<DefaultBatch>();
            exceptions = new ConcurrentQueue<Exception>();

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
                if (!exceptions.IsEmpty)
                {
                    throw new AggregateException(exceptions);
                }

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
                var exception = new XunitException("Posted content stream should not contain UTF8 BOM");
                exceptions.Enqueue(exception);

                throw exception;
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
                var exception = new XunitException($"{nameof(HttpClientMock)} assume log events are formatted using {nameof(NormalRenderedTextFormatter)}, and batches are formatted using {nameof(DefaultBatchFormatter)}");
                exceptions.Enqueue(exception);

                throw exception;
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
