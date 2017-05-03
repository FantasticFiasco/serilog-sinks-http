using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Support
{
    public class TextWriterSink : ILogEventSink
    {
        readonly StringWriter output;
        readonly ITextFormatter formatter;

        public TextWriterSink(StringWriter output, ITextFormatter formatter)
        {
            this.output = output;
            this.formatter = formatter;
        }

        public void Emit(LogEvent logEvent)
        {
            formatter.Format(logEvent, output);
        }
    }
}
