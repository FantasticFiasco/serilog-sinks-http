using Newtonsoft.Json;
using Serilog.Support.TextFormatters;

namespace Serilog.Support.BatchFormatters
{
    // TODO: Remove this class, since a duplicate can be found in the log server project
    public class DefaultBatch
    {
        [JsonProperty("events")]
        public NormalTextLogEvent[] Events { get; set; }
    }
}
