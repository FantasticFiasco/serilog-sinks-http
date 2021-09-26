using Newtonsoft.Json;
using Shouldly;
using System.IO;
using System.Linq;
using Serilog.Support;
using Serilog.Support.BatchFormatters;
using Xunit;

namespace Serilog.Sinks.Http.BatchFormatters
{
    public class DefaultBatchFormatterShould
    {
        private readonly string[] logEvents;
        private readonly StringWriter output;

        public DefaultBatchFormatterShould()
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
            var batchFormatter = new DefaultBatchFormatter();

            // Act
            batchFormatter.Format(logEvents, output);

            // Assert
            var got = JsonConvert.DeserializeObject<DefaultBatch>(output.ToString());

            got.Events.Length.ShouldBe(2);
            got.Events[0].RenderedMessage.ShouldBe("Event 1");
            got.Events[1].RenderedMessage.ShouldBe("Event 2");
        }

        [Fact]
        public void HandleEmptySequenceOfLogEvents()
        {
            // Arrange
            var batchFormatter = new DefaultBatchFormatter();
            var emptySequenceOfLogEvents = Enumerable.Empty<string>();

            // Act
            batchFormatter.Format(emptySequenceOfLogEvents, output);

            // Assert
            var got = output.ToString();
            got.ShouldBeEmpty();
        }
    }
}
