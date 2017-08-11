using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;
using Serilog.Sinks.Http.TextFormatters;
using Serilog.Support;
using Shouldly;
using System.IO;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public class DefaultBatchFormatterTest
    {
        private readonly LogEvent[] logEvents;
        private readonly ITextFormatter textFormatter;
        private readonly StringWriter output;

        public DefaultBatchFormatterTest()
        {
            logEvents = new LogEvent[] { Some.DebugEvent(), Some.DebugEvent() };
            textFormatter = new NormalTextFormatter();
            output = new StringWriter();
        }

        [Fact]
        public void FormatLogEvents()
        {
            // Arrange
            var batchFormatter = new DefaultBatchFormatter();

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var eventBatch = JsonConvert.DeserializeObject<EventBatchRequestDto>(output.ToString());
            eventBatch.Events.Count().ShouldBe(logEvents.Count());
        }

        [Fact]
        public void FormatFormattedLogEvents()
        {
            // Arrange
            var batchFormatter = new DefaultBatchFormatter();
            var formattedLogEvents = logEvents.Select(logEvent =>
                {
                    var formattedLogEvent = new StringWriter();
                    textFormatter.Format(logEvent, formattedLogEvent);
                    return formattedLogEvent.ToString();
                });

            // Act
            batchFormatter.Format(formattedLogEvents, output);

            // Assert
            var eventBatch = JsonConvert.DeserializeObject<EventBatchRequestDto>(output.ToString());
            eventBatch.Events.Count().ShouldBe(logEvents.Count());
        }

        [Fact]
        public void DropWhenSizeExceedsMaximum()
        {
            // Arrange
            var batchFormatter = new DefaultBatchFormatter(0);

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var eventBatch = JsonConvert.DeserializeObject<EventBatchRequestDto>(output.ToString());
            eventBatch.Events.Count().ShouldBe(0);
        }
    }
}
