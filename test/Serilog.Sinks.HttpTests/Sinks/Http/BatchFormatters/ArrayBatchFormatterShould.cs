using Newtonsoft.Json;
using Shouldly;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Http.TextFormatters;
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
            textFormatter = new NormalRenderedTextFormatter();
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
            var got = JsonConvert.DeserializeObject<NormalTextLogEvent[]>(output.ToString());

            got.Length.ShouldBe(2);
            got[0].RenderedMessage.ShouldBe("Event 1");
            got[1].RenderedMessage.ShouldBe("Event 2");
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
            var got = JsonConvert.DeserializeObject<NormalTextLogEvent[]>(output.ToString());

            got.Length.ShouldBe(2);
            got[0].RenderedMessage.ShouldBe("Event 1");
            got[1].RenderedMessage.ShouldBe("Event 2");
        }

        [Fact]
        public void DropLogEventsGivenSizeExceedsMaximum()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter(1);

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var got = JsonConvert.DeserializeObject<NormalTextLogEvent[]>(output.ToString());
            got.ShouldBeEmpty();
        }
    }
}
