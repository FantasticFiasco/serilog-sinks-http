using Serilog.Support.TextFormatters;

namespace Serilog.Support.BatchFormatters
{
    public class DefaultBatch
    {
        public NormalTextLogEvent[] Events { get; set; }
    }
}
