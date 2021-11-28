using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Serilog.Sinks.HttpTests.LogServer;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Support.Fixtures
{
    public class WebServerFixture : WebApplicationFactory<Startup>
    {
        public string RequestUri(string testId)
        {
            var requestUri = Server.BaseAddress + "logs/" + WebUtility.UrlEncode(testId);
            return requestUri;
        }

        public void SimulateNetworkFailure(TimeSpan duration)
        {
            var setIsHealthy = (bool isHealthy) => GetHealthService().SetIsHealthy(isHealthy);

            setIsHealthy(false);

            Task.Factory.StartNewAfterDelay(
                duration,
                () => setIsHealthy(true));
        }

        public Task ExpectBatches(string testId, int numberOfBatches)
        {
            return ExpectBatches(testId, numberOfBatches, TimeSpan.FromSeconds(10));
        }

        public async Task ExpectBatches(string testId, int numberOfBatches, TimeSpan timeout)
        {
            var deadline = DateTime.Now.Add(timeout);

            while (DateTime.Now < deadline)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                var got = GetAllBatches(testId).Length;

                if (got < numberOfBatches) continue;
                else if (got == numberOfBatches) return;
                else throw new Exception($"Got {got} number of batches; want {numberOfBatches}");
            }

            throw new Exception($"Timed out while expecting {numberOfBatches} batche(s)");
        }

        public Task ExpectLogEvents(string testId, int numberOfLogEvents)
        {
            return ExpectLogEvents(testId, numberOfLogEvents, TimeSpan.FromSeconds(10));
        }

        public async Task ExpectLogEvents(string testId, int numberOfLogEvents, TimeSpan timeout)
        {
            var deadline = DateTime.Now.Add(timeout);

            while (DateTime.Now < deadline)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));

                var got = GetAllEvents(testId).Length;

                if (got < numberOfLogEvents) continue;
                else if (got == numberOfLogEvents) return;
                else throw new Exception($"Got {got} number of events; want {numberOfLogEvents}");                
            }

            throw new Exception($"Timed out while expecting {numberOfLogEvents} log event(s)");
        }

        public LogEvent[][] GetAllBatches(string testId)
        {
            return GetLogEventService().GetAllBatches(testId);
        }

        public LogEvent[] GetAllEvents(string testId)
        {
            return GetLogEventService().GetAllEvents(testId);
        }

        private HealthService GetHealthService()
        {
            return (HealthService)Services.GetService(typeof(HealthService));
        }

        private LogEventService GetLogEventService()
        {
            return (LogEventService)Services.GetService(typeof(LogEventService));
        }
    }
}
