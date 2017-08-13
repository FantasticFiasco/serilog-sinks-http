
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using System.IO;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public abstract class BatchFormatterFixture
    {
        protected readonly LogEvent[] logEvents;
        protected readonly ITextFormatter textFormatter;
        protected readonly StringWriter output;

        protected BatchFormatterFixture()
        {
            logEvents = new LogEvent[] { Some.DebugEvent(), Some.DebugEvent() };
            textFormatter = new NormalTextFormatter();
            output = new StringWriter();
        }
    }
}
