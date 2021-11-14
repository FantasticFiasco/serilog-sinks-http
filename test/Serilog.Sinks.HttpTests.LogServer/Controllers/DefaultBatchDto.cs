using System.Text.Json.Serialization;

namespace Serilog.Sinks.HttpTests.LogServer.Controllers
{
    public class DefaultBatchDto
    {
        [JsonPropertyName("events")]
        public LogEventDto[] Events { get; set; } = new LogEventDto[0];
    }
}
