using Newtonsoft.Json;
using Serilog.Sinks.Http.LogServer.Controllers.Dto;
using Shouldly;
using System.IO;
using System.Linq;
using Xunit;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public class ArrayBatchFormatterTest : BatchFormatterFixture
    {
        [Fact]
        public void FormatLogEvents()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter();

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var eventBatch = JsonConvert.DeserializeObject<EventDto[]>(output.ToString());
            eventBatch.Count().ShouldBe(logEvents.Count());
        }

        [Fact]
        public void FormatFormattedLogEvents()
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
            var eventBatch = JsonConvert.DeserializeObject<EventDto[]>(output.ToString());
            eventBatch.Count().ShouldBe(logEvents.Count());
        }

        [Fact]
        public void DropWhenSizeExceedsMaximum()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter(0);

            // Act
            batchFormatter.Format(logEvents, textFormatter, output);

            // Assert
            var eventBatch = JsonConvert.DeserializeObject<EventDto[]>(output.ToString());
            eventBatch.Count().ShouldBe(0);
        }
    }
}
