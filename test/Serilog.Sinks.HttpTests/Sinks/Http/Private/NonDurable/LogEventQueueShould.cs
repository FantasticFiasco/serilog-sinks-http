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
            var dequeueSuccess1 = queue.TryDequeue(out var got1);
            var dequeueSuccess2 = queue.TryDequeue(out var got2);
            var dequeueSuccess3 = queue.TryDequeue(out var got3);
            var dequeueSuccess4 = queue.TryDequeue(out var got4);

            // Assert
            dequeueSuccess1.ShouldBeTrue();
            dequeueSuccess2.ShouldBeTrue();
            dequeueSuccess3.ShouldBeTrue();
            dequeueSuccess4.ShouldBeFalse();

            got1.ShouldBe("1");
            got2.ShouldBe("2");
            got3.ShouldBe("3");
            got4.ShouldBeNull();
        }

        [Fact]
        public void RespectQueueLimit()
        {
            // Arrange
            var queue = new LogEventQueue(3);
            queue.Enqueue("1");
            queue.Enqueue("2");
            queue.Enqueue("3");

            // Act
            var enqueueSuccess = queue.TryEnqueue("4");

            // Assert
            enqueueSuccess.ShouldBeFalse();
        }
    }
}
