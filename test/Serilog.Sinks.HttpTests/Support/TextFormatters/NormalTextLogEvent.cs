using System;

namespace Serilog.Support.TextFormatters
{
    public class NormalTextLogEvent : IEquatable<NormalTextLogEvent>
    {
        public string RenderedMessage { get; set; }

        public bool Equals(NormalTextLogEvent other) =>
            other != null && other.RenderedMessage == RenderedMessage;

        public override bool Equals(object obj) =>
            Equals(obj as NormalTextLogEvent);

        public override int GetHashCode() =>
            0;
    }
}
