using System.Collections.Concurrent;

namespace Serilog.Sinks.HttpTests.LogServer.Services
{
    public class LogEventService
    {
        private readonly BlockingCollection<string> _logEvents;

        public LogEventService()
        {
            _logEvents = new BlockingCollection<string>();
        }

        public void Add(string logEvent)
        {
            _logEvents.Add(logEvent);
        }

        public string[] GetAll()
        {
            return _logEvents.ToArray();
        }
    }
}
