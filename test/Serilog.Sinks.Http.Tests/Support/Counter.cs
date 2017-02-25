using System;
using System.Threading;
using Xunit;

namespace Serilog.Sinks.Http.Support
{
    internal class Counter
    {
		private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

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

			SetEventIfCounterEqualsExpected(current);
        }

	    public void Add(int value)
	    {
			var current = Interlocked.Add(ref actual, value);

			SetEventIfCounterEqualsExpected(current);
		}

        public void Wait(TimeSpan? timeout = null)
        {
	        timeout = timeout ?? DefaultTimeout;

			var success = resetEvent.Wait(timeout.Value);
            Assert.True(success, $"Expected to count to {expected} but only got to {actual} before timeout {timeout}");
        }

	    private void SetEventIfCounterEqualsExpected(int current)
	    {
			Assert.True(current <= expected, $"Current count {current} is greater than expected count {expected}");

			if (current == expected)
			{
				resetEvent.Set();
			}
		}
    }
}