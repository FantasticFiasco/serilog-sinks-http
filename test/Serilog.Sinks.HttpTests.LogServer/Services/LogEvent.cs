using System;

namespace Serilog.Sinks.HttpTests.LogServer.Services
{
    public class LogEvent
    {
        public LogEvent(DateTime timestamp)
        {
            Timestamp = timestamp;
        }

        public DateTime Timestamp { get; }
    }
}
