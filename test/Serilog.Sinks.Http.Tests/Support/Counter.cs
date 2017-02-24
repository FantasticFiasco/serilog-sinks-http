using System;
using System.Threading;
using Xunit;

namespace Serilog.Sinks.Http.Support
{
    internal class Counter
    {
        private readonly int expected;
        private readonly ManualResetEventSlim resetEvent;

        private int actual;

        public Counter(int expected)
        {
            if (expected <= 0)
                throw new ArgumentException("expected must be at least 1");

            this.expected = expected;
            resetEvent = new ManualResetEventSlim();
        }

        public void Increment()
        {
            var current = Interlocked.Increment(ref actual);
            if (current == expected)
            {
                resetEvent.Set();
            }
        }

        public void Wait(TimeSpan timeout)
        {
            var success = resetEvent.Wait(timeout);
            Assert.True(success, $"Expected to count to {expected} but only got to {actual} before timeout {timeout}");
        }
    }
}