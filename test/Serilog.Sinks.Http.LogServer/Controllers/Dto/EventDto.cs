using System;
using System.Collections.Generic;

namespace Serilog.Sinks.Http.LogServer.Controllers.Dto
{
    public class EventDto
    {
        public DateTime Timestamp { get; set; }

        public string Level { get; set; }

        public string MessageTemplate { get; set; }

        public string RenderedMessage { get; set; }

        public string Exception { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public Dictionary<string, RenderingDto[]> Renderings { get; set; }
    }
}