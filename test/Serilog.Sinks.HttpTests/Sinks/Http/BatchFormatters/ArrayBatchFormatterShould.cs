using Newtonsoft.Json;
using Shouldly;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Support;
using Serilog.Support.TextFormatters;
using Xunit;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public class ArrayBatchFormatterShould
    {
        private readonly LogEvent[] logEvents;
        private readonly ITextFormatter textFormatter;
        private readonly StringWriter output;

        public ArrayBatchFormatterShould()
        {
            logEvents = new[] { Some.LogEvent("Event {number}", 1), Some.LogEvent("Event {number}", 2) };
            textFormatter = new RenderedMessageTextFormatter();
            output = new StringWriter();
        }

        [Fact]
        public void WriteLogEvents()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter();

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var actual = JsonConvert.DeserializeObject<string[]>(output.ToString());
            actual.ShouldBe(new[] { "Event 1", "Event 2" });
        }

        [Fact]
        public void WriteFormattedLogEvents()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter();

            var formattedLogEvents = logEvents.Select(logEvent =>
            {
                var formattedLogEvent = new StringWriter();
                textFormatter.Format(logEvent, formattedLogEvent);
                return formattedLogEvent.ToString();
            });

            // Act
            batchFormatter.Format(formattedLogEvents, output);

            // Assert
            var actual = JsonConvert.DeserializeObject<string[]>(output.ToString());
            actual.ShouldBe(new[] { "Event 1", "Event 2" });
        }

        [Fact]
        public void DropLogEventsGivenSizeExceedsMaximum()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter(1);

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var actual = JsonConvert.DeserializeObject<string[]>(output.ToString());
            actual.ShouldBeEmpty();
        }
    }
}
