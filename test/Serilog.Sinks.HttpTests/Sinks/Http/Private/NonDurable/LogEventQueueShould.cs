using System;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.NonDurable
{
    public class LogEventQueueShould
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public void ThrowExceptionGivenInvalidQueueLimitBytes(long queueLimitBytes)
        {
            // Arrange
            // ReSharper disable once ObjectCreationAsStatement
            var got = new Action(() => new LogEventQueue(queueLimitBytes));

            // Assert
            got.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void UseFifo()
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");

            // Act
            var dequeueResult1 = queue.TryDequeue(null, out var got1);
            var dequeueResult2 = queue.TryDequeue(null, out var got2);
            var dequeueResult3 = queue.TryDequeue(null, out var got3);
            var dequeueResult4 = queue.TryDequeue(null, out var got4);

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
        public void ThrowExceptionGivenFullQueue()
        {
            // Arrange
            var queue = new LogEventQueue(3);
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");

            // Act
            Action got = () => queue.Enqueue("4");

            // Assert
            got.ShouldThrow<Exception>();
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
            var queue = new LogEventQueue();
            queue.Enqueue("1");

            // Act
            var result = queue.TryDequeue(null, out var got);

            // Assert
            result.ShouldBe(LogEventQueue.DequeueResult.Ok);
            got.ShouldBe("1");
        }

        [Fact]
        public void NotDequeueGivenEmptyQueue()
        {
            // Arrange
            var queue = new LogEventQueue();

            // Act
            var result = queue.TryDequeue(null, out var got);

            // Assert
            result.ShouldBe(LogEventQueue.DequeueResult.QueueEmpty);
            got.ShouldBe(string.Empty);
        }

        [Theory]
        [InlineData("0123456789", 8, LogEventQueue.DequeueResult.MaxSizeViolation)]
        [InlineData("0123456789", 9, LogEventQueue.DequeueResult.MaxSizeViolation)]
        [InlineData("0123456789", 10, LogEventQueue.DequeueResult.Ok)]
        [InlineData("0123456789", 11, LogEventQueue.DequeueResult.Ok)]
        public void DequeueGivenMaxSize(string logEvent, int logEventMaxSize, LogEventQueue.DequeueResult want)
        {
            // Arrange
            var queue = new LogEventQueue();
            queue.Enqueue(logEvent);

            // Act
            var got = queue.TryDequeue(logEventMaxSize, out var dequeuedLogEvent);

            // Assert
            got.ShouldBe(want);
            dequeuedLogEvent.ShouldBe(want == LogEventQueue.DequeueResult.Ok ? logEvent : string.Empty);
        }
    }
}
