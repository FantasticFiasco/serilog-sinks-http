using System;

namespace Serilog.Support
{
    public class RenderedMessageLogEvent : IEquatable<RenderedMessageLogEvent>
    {
        public string RenderedMessage { get; set; }

        public bool Equals(RenderedMessageLogEvent other) =>
            other != null && other.RenderedMessage == RenderedMessage;

        public override bool Equals(object obj) =>
            Equals(obj as RenderedMessageLogEvent);

        public override int GetHashCode() =>
            0;
    }
}
