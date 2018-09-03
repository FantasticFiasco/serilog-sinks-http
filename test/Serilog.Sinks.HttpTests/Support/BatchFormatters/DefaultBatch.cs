using Newtonsoft.Json;
using Serilog.Support.TextFormatters;

namespace Serilog.Support.BatchFormatters
{
    public class DefaultBatch
    {
        [JsonProperty("events")]
        public NormalTextLogEvent[] Events { get; set; }
    }
}
