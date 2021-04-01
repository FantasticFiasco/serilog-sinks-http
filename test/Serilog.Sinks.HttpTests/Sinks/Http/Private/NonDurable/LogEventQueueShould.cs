using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class LogEventQueueShould
    {
        [Fact]
        public void UseFifo()
        {
            // Arrange
            var queue = new LogEventQueue(null);
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");

            // Act
            var dequeueResult1 = queue.TryDequeue(long.MaxValue, out var got1);
            var dequeueResult2 = queue.TryDequeue(long.MaxValue, out var got2);
            var dequeueResult3 = queue.TryDequeue(long.MaxValue, out var got3);
            var dequeueResult4 = queue.TryDequeue(long.MaxValue, out var got4);

            // Assert
            dequeueResult1.ShouldBe(LogEventQueue.DequeueResult.Ok);
            dequeueResult2.ShouldBe(LogEventQueue.DequeueResult.Ok);
            dequeueResult3.ShouldBe(LogEventQueue.DequeueResult.Ok);
            dequeueResult4.ShouldBe(LogEventQueue.DequeueResult.QueueEmpty);

            got1.ShouldBe("1");
            got2.ShouldBe("2");
            got3.ShouldBe("3");
            got4.ShouldBe("");
        }

        [Fact]
        public void NotEnqueueGivenFullQueue()
        {
            // Arrange
            var queue = new LogEventQueue(3);
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");

            // Act
            var got = queue.TryEnqueue("4");

            // Assert
            got.ShouldBe(LogEventQueue.EnqueueResult.QueueFull);
        }

        [Fact]
        public void Dequeue()
        {
            // Arrange
            var queue = new LogEventQueue(null);
            queue.Enqueue("1");

            // Act
            var result = queue.TryDequeue(long.MaxValue, out var got);

            // Assert
            result.ShouldBe(LogEventQueue.DequeueResult.Ok);
            got.ShouldBe("1");
        }

        [Fact]
        public void NotDequeueGivenEmptyQueue()
        {
            // Arrange
            var queue = new LogEventQueue(null);

            // Act
            var result = queue.TryDequeue(long.MaxValue, out var got);

            // Assert
            result.ShouldBe(LogEventQueue.DequeueResult.QueueEmpty);
            got.ShouldBe("");
        }

        [Theory]
        [InlineData("0123456789", 9, LogEventQueue.DequeueResult.MaxSizeViolation)]
        [InlineData("0123456789", 10, LogEventQueue.DequeueResult.Ok)]
        [InlineData("0123456789", 11, LogEventQueue.DequeueResult.Ok)]
        public void DequeueGivenMaxSize(string logEvent, int maxSize, LogEventQueue.DequeueResult want)
        {
            // Arrange
            var queue = new LogEventQueue(null);
            queue.Enqueue(logEvent);

            // Act
            var got = queue.TryDequeue(maxSize, out var dequeuedLogEvent);

            // Assert
            got.ShouldBe(want);
            dequeuedLogEvent.ShouldBe(want == LogEventQueue.DequeueResult.Ok ? logEvent : "");
        }
    }
}
