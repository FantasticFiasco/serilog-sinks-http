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
            var period = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(period);

            // Act
            var actual = schedule.NextInterval;

            // Assert
            actual.ShouldBe(period);
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
            var period = TimeSpan.FromSeconds(periodInSeconds);
            var schedule = new ExponentialBackoffConnectionSchedule(period);

            schedule.MarkFailure();

            // Act
            var actual = schedule.NextInterval;

            // Assert
            actual.ShouldBe(period);
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
            // // Arrange
            // var period = TimeSpan.FromSeconds(periodInSeconds);
            // var schedule = new ExponentialBackoffConnectionSchedule(period);

            throw new NotImplementedException();
        }

        private interface IBackoff
        {
            IBackoff GetNext(IBackoff previous);
        }

        /// <summary>
        /// Backoff might appear as linear in the beginning.
        /// </summary>
        private class LinearBackoffState : IBackoff
        {
            public IBackoff GetNext(IBackoff previous) => throw new NotImplementedException();
        }

        /// <summary>
        /// Backoff is exponential.
        /// </summary>
        private class ExponentialBackoffState : IBackoff
        {
            public IBackoff GetNext(IBackoff previous) => throw new NotImplementedException();
        }

        /// <summary>
        /// Backoff is capped to a maximum value.
        /// </summary>
        private class CappedBackoffState : IBackoff
        {
            public IBackoff GetNext(IBackoff previous) => throw new NotImplementedException();
        }
    }
}
