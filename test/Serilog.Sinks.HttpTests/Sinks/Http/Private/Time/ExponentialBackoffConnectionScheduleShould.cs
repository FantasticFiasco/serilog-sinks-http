using System;
using Shouldly;
using Xunit;

namespace Serilog.Sinks.Http.Private.Time
{
    public class ExponentialBackoffConnectionScheduleShould
    {
        [Theory]
        [InlineData(1)]        // 1s
        [InlineData(2)]        // 2s
        [InlineData(5)]        // 5s
        [InlineData(10)]       // 10s
        [InlineData(30)]       // 30s
        [InlineData(1 * 60)]   // 1 min
        [InlineData(5 * 60)]   // 5 min
        [InlineData(10 * 60)]  // 10 min
        public void ReturnPeriodDuringSuccess(int periodInSeconds)
        {
            // Arrange
            var want = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(want);

            // Act
            var got = schedule.NextInterval;

            // Assert
            got.ShouldBe(want);
        }

        [Theory]
        [InlineData(1)]        // 1s
        [InlineData(2)]        // 2s
        [InlineData(5)]        // 5s
        [InlineData(10)]       // 10s
        [InlineData(30)]       // 30s
        [InlineData(1 * 60)]   // 1 min
        [InlineData(5 * 60)]   // 5 min
        [InlineData(10 * 60)]  // 10 min
        public void ReturnPeriodAfterFirstFailure(int periodInSeconds)
        {
            // Arrange
            var want = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(want);

            schedule.MarkFailure();

            // Act
            var got = schedule.NextInterval;

            // Assert
            got.ShouldBe(want);
        }

        [Theory]
        [InlineData(1)]        // 1s
        [InlineData(2)]        // 2s
        [InlineData(5)]        // 5s
        [InlineData(10)]       // 10s
        [InlineData(30)]       // 30s
        [InlineData(1 * 60)]   // 1 min
        [InlineData(5 * 60)]   // 5 min
        [InlineData(10 * 60)]  // 10 min
        public void BehaveExponentially(int periodInSeconds)
        {
            // Arrange
            var period = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(period);
            IBackoff backoff = new LinearBackoff(period);

            while (!(backoff is CappedBackoff))
            {
                // Act
                schedule.MarkFailure();

                // Assert
                backoff = backoff.GetNext(schedule.NextInterval);
            }
        }

        [Theory]
        [InlineData(1)]        // 1s
        [InlineData(2)]        // 2s
        [InlineData(5)]        // 5s
        [InlineData(10)]       // 10s
        [InlineData(30)]       // 30s
        [InlineData(1 * 60)]   // 1 min
        [InlineData(5 * 60)]   // 5 min
        [InlineData(10 * 60)]  // 10 min
        public void RemainCappedDuringFailures(int periodInSeconds)
        {
            // Arrange
            var period = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(period);

            // Lets make sure the backoff is capped
            while (schedule.NextInterval != ExponentialBackoffConnectionSchedule.MaximumBackoffInterval)
            {
                schedule.MarkFailure();
            }

            // Act
            for (var i = 0; i < 100000; i++)    // 100 000 failures is the result of almost two years of downtime
            {
                // Assert
                if (schedule.NextInterval != ExponentialBackoffConnectionSchedule.MaximumBackoffInterval)
                {
                    throw new Exception($"Backoff schedule transitioned from being capped ({ExponentialBackoffConnectionSchedule.MaximumBackoffInterval}) to no longer being capped ({schedule.NextInterval})");
                }

                schedule.MarkFailure();
            }
        }

        private interface IBackoff
        {
            IBackoff GetNext(TimeSpan nextInterval);
        }

        /// <summary>
        /// An exponential backoff implementation might appear as linear in the start.
        /// </summary>
        private class LinearBackoff : IBackoff
        {
            private readonly TimeSpan currentInterval;

            public LinearBackoff(TimeSpan currentInterval)
            {
                this.currentInterval = currentInterval;
            }

            public IBackoff GetNext(TimeSpan nextInterval)
            {
                // From the state of being linear, the implementation can become capped
                if (nextInterval == ExponentialBackoffConnectionSchedule.MaximumBackoffInterval)
                {
                    return new CappedBackoff(nextInterval);
                }

                // From the state of being linear, the implementation can become exponential
                if (nextInterval > currentInterval)
                {
                    return new ExponentialBackoff(nextInterval);
                }

                // From the state of being linear, the implementation can remain linear
                if (nextInterval == currentInterval)
                {
                    return this;
                }

                throw new Exception("The implementation from being linear must remain linear or become exponential");
            }
        }

        /// <summary>
        /// An exponential backoff implementation.
        /// </summary>
        private class ExponentialBackoff : IBackoff
        {
            private readonly TimeSpan currentInterval;

            public ExponentialBackoff(TimeSpan currentInterval)
            {
                this.currentInterval = currentInterval;
            }

            public IBackoff GetNext(TimeSpan nextInterval)
            {
                // From the state of being exponential, the implementation can become capped
                if (nextInterval == ExponentialBackoffConnectionSchedule.MaximumBackoffInterval)
                {
                    return new CappedBackoff(nextInterval);
                }

                // From the state of being exponential, the implementation can remain exponential
                if (nextInterval > currentInterval)
                {
                    return new ExponentialBackoff(nextInterval);
                }

                throw new Exception();
            }
        }

        /// <summary>
        /// An exponential backoff implementation might be capped in the end.
        /// </summary>
        private class CappedBackoff : IBackoff
        {
            private readonly TimeSpan currentInterval;

            public CappedBackoff(TimeSpan currentInterval)
            {
                this.currentInterval = currentInterval;
            }

            public IBackoff GetNext(TimeSpan nextInterval)
            {
                if (nextInterval != currentInterval)
                {
                    throw new Exception("Once backoff implementation is capped, it should remain capped");
                }

                return this;
            }
        }
    }
}
