using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class LogEventQueueReaderShould
    {
        private const string FooLogEvent = "{ \"foo\": 1 }";
        private const string BarLogEvent = "{ \"bar\": 2 }";

        [Fact]
        public void ReadLogEvent()
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue(FooLogEvent);

            // Act
            var got = LogEventQueueReader.Read(queue, null, null);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
        }

        [Fact]
        public void ReadLogEvents()
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue(FooLogEvent);
            queue.Enqueue(BarLogEvent);

            // Act
            var got = LogEventQueueReader.Read(queue, null, null);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent, BarLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
        }

        [Fact]
        public void RespectBatchPostingLimit()
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue(FooLogEvent);
            queue.Enqueue(BarLogEvent);

            const int batchPostingLimit = 1;

            // Act
            var got = LogEventQueueReader.Read(queue, batchPostingLimit, null);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeTrue();
        }

        [Fact]
        public void RespectBatchSizeLimit()
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue(FooLogEvent);
            queue.Enqueue(BarLogEvent);

            var batchSizeLimit = (FooLogEvent.Length + BarLogEvent.Length) * 2 / 3;

            // Act
            var got = LogEventQueueReader.Read(queue, null, batchSizeLimit);

            // Assert
            got.LogEvents.ShouldBe(new[] { FooLogEvent });
            got.HasReachedLimit.ShouldBeTrue();
        }

        [Fact]
        public void SkipLogEventGivenItExceedsBatchSizeLimit()
        {
            // Arrange
            const string logEventExceedingBatchSizeLimit = "{ \"foo\": \"This document exceeds the batch size limit\" }";

            var queue = new LogEventQueue();
            queue.Enqueue(logEventExceedingBatchSizeLimit);
            queue.Enqueue(BarLogEvent);

            var batchSizeLimit = ByteSize.From(logEventExceedingBatchSizeLimit) - 1;

            // Act
            var got = LogEventQueueReader.Read(queue, null, batchSizeLimit);

            // Assert
            got.LogEvents.ShouldBe(new[] { BarLogEvent });
            got.HasReachedLimit.ShouldBeFalse();
        }
    }
}
