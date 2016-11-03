using System;
using System.Linq;
using System.Threading;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Sinks.Http.Tests.Support
{
    internal static class Some
    {
        private static int counter;

        internal static int Int()
        {
            return Interlocked.Increment(ref counter);
        }

        internal static string String(string tag = null)
        {
            return (tag ?? "") + "__" + Int();
        }

        internal static TimeSpan TimeSpan()
        {
            return System.TimeSpan.FromMinutes(Int());
        }

        internal static DateTime Instant()
        {
            return new DateTime(2012, 10, 28) + TimeSpan();
        }

        internal static DateTimeOffset OffsetInstant()
        {
            return new DateTimeOffset(Instant());
        }

        internal static LogEvent LogEvent(DateTimeOffset? timestamp = null, LogEventLevel level = LogEventLevel.Information)
        {
            return new LogEvent(
                timestamp ?? OffsetInstant(),
                level,
                null,
                MessageTemplate(),
                Enumerable.Empty<LogEventProperty>());
        }

        internal static LogEvent InformationEvent(DateTimeOffset? timestamp = null)
        {
            return LogEvent(timestamp);
        }

        internal static LogEvent DebugEvent(DateTimeOffset? timestamp = null)
        {
            return LogEvent(timestamp, LogEventLevel.Debug);
        }

        internal static MessageTemplate MessageTemplate()
        {
            return new MessageTemplateParser().Parse(String());
        }
    }
}
