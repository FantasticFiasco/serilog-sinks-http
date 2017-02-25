using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using Serilog.Sinks.Http.Private;
using Serilog.Sinks.Http.Support;
using Xunit;

namespace Serilog.Sinks.Http.Sinks.Http.Private
{
    public class HttpSinkTest
    {
        private readonly Mock<IHttpClient> client;
        private readonly string requestUri;
        private readonly HttpSink sink;

        public HttpSinkTest()
        {
            client = new Mock<IHttpClient>();
            requestUri = "www.mylogs.com";
            sink = new HttpSink(
                client.Object,
                requestUri,
				100,							// batchPostingLimit
				TimeSpan.FromMilliseconds(50),	// period
				null);
        }

        [Theory]
		[InlineData(1)]			// 1 batch
		[InlineData(10)]		// 1 batch
		[InlineData(100)]		// ~1 batch
		[InlineData(1000)]		// ~10 batches
		[InlineData(10000)]		// ~100 batches
		[InlineData(100000)]	// ~1000 batches
		public void Emit(int numberOfEvents)
        {
            // Arrange
            var counter = new Counter(numberOfEvents);

			SetupCountingPostedEvents(counter);

            // Act
			Send(numberOfEvents);

			// Assert
            counter.Wait();
        }

	    private void SetupCountingPostedEvents(Counter counter)
	    {
			client
				.Setup(mock => mock.PostAsync(requestUri, It.IsAny<HttpContent>()))
				.Callback<string, HttpContent>(
					async (_, httpContent) =>
					{
						int count = await ReadNumberOfEventsAsync(httpContent);
						counter.Add(count);
					})
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
		}

	    private void Send(int numberOfEvents)
	    {
			for (int i = 0; i < numberOfEvents; i++)
			{
				sink.Emit(Some.DebugEvent());
			}
		}

		private static async Task<int> ReadNumberOfEventsAsync(HttpContent httpContent)
	    {
		    var eventsDto = JsonConvert.DeserializeObject<EventsDto>(await httpContent.ReadAsStringAsync());

			return eventsDto.events.Count();
	    }
    }
}
