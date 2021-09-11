using Newtonsoft.Json;
using Shouldly;
using System.IO;
using System.Linq;
using Serilog.Support;
using Serilog.Support.TextFormatters;
using Xunit;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public class ArrayBatchFormatterShould
    {
        private readonly string[] logEvents;
        private readonly StringWriter output;

        public ArrayBatchFormatterShould()
        {
            logEvents = new[]
            {
                Some.SerializedLogEvent("Event {number}", 1),
                Some.SerializedLogEvent("Event {number}", 2)
            };
            output = new StringWriter();
        }

        [Fact]
        public void WriteLogEvents()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter();

            // Act
            batchFormatter.Format(logEvents, output);

            // Assert
            var got = JsonConvert.DeserializeObject<NormalTextLogEvent[]>(output.ToString());

            got.Length.ShouldBe(2);
            got[0].RenderedMessage.ShouldBe("Event 1");
            got[1].RenderedMessage.ShouldBe("Event 2");
        }

        [Fact]
        public void HandleEmptySequenceOfLogEvents()
        {
            // Arrange
            var batchFormatter = new ArrayBatchFormatter();
            var emptySequenceOfLogEvents = Enumerable.Empty<string>();

            // Act
            batchFormatter.Format(emptySequenceOfLogEvents, output);

            // Assert
            var got = output.ToString();
            got.ShouldBeEmpty();
        }

        // [Fact]
        // public void DropLogEventsGivenSizeExceedsMaximum()
        // {
        //     // Arrange
        //     var batchFormatter = new ArrayBatchFormatter(1);
        //
        //     // Act
        //     batchFormatter.Format(logEvents, output);
        //
        //     // Assert
        //     var got = JsonConvert.DeserializeObject<NormalTextLogEvent[]>(output.ToString());
        //     got.ShouldBeEmpty();
        // }
    }
}
