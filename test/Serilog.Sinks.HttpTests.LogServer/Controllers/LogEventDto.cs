using System;
using Serilog.Sinks.HttpTests.LogServer.Services;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    public class LogEventDto
    {
        public DateTime Timestamp { get; set; }

        public LogEvent ToLogEvent()
        {
            return new LogEvent(Timestamp);
        }

        public static LogEventDto From(LogEvent logEvent)
        {
            return new LogEventDto
            {
                Timestamp = logEvent.Timestamp
            };
        }
    }
}
