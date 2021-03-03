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
            var enqueueSuccess1 = queue.TryEnqueue("1");
            var enqueueSuccess2 = queue.TryEnqueue("2");
            var enqueueSuccess3 = queue.TryEnqueue("3");

            // Act
            var dequeueSuccess1 = queue.TryDequeue(out var got1);
            var dequeueSuccess2 = queue.TryDequeue(out var got2);
            var dequeueSuccess3 = queue.TryDequeue(out var got3);
            var dequeueSuccess4 = queue.TryDequeue(out var got4);

            // Assert
            enqueueSuccess1.ShouldBeTrue();
            enqueueSuccess2.ShouldBeTrue();
            enqueueSuccess3.ShouldBeTrue();

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
            var enqueueSuccess1 = queue.TryEnqueue("1");
            var enqueueSuccess2 = queue.TryEnqueue("2");
            var enqueueSuccess3 = queue.TryEnqueue("3");

            // Act
            var enqueueSuccess4 = queue.TryEnqueue("4");

            // Assert
            enqueueSuccess1.ShouldBeTrue();
            enqueueSuccess2.ShouldBeTrue();
            enqueueSuccess3.ShouldBeTrue();
            enqueueSuccess4.ShouldBeFalse();
        }
    }
}
