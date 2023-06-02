using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Support.Sinks;

public class TextWriterSink : ILogEventSink
{
    private readonly StringWriter output;
    private readonly ITextFormatter formatter;

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