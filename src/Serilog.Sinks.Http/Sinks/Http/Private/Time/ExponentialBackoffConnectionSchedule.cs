// Copyright 2015-2016 Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Serilog.Sinks.Http.Private.Time
{
	internal class ExponentialBackoffConnectionSchedule
	{
		private static readonly TimeSpan MinimumBackoffPeriod = TimeSpan.FromSeconds(5);
		private static readonly TimeSpan MaximumBackoffInterval = TimeSpan.FromMinutes(10);

		private readonly TimeSpan period;

		private int failuresSinceSuccessfulConnection;

		public ExponentialBackoffConnectionSchedule(TimeSpan period)
		{
			if (period < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(period), "The connection retry period must be a positive timespan");

			this.period = period;
		}

		public void MarkSuccess()
		{
			failuresSinceSuccessfulConnection = 0;
		}

		public void MarkFailure()
		{
			failuresSinceSuccessfulConnection++;
		}

		public TimeSpan NextInterval
		{
			get
			{
				// Available, and first failure, just try the batch interval
				if (failuresSinceSuccessfulConnection <= 1)
					return period;

				// Second failure, start ramping up the interval - first 2x, then 4x, ...
				var backoffFactor = Math.Pow(2, failuresSinceSuccessfulConnection - 1);

				// If the period is ridiculously short, give it a boost so we get some
				// visible backoff
				var backoffPeriod = Math.Max(period.Ticks, MinimumBackoffPeriod.Ticks);

				// The "ideal" interval
				var backedOff = (long)(backoffPeriod * backoffFactor);

				// Capped to the maximum interval
				var cappedBackoff = Math.Min(MaximumBackoffInterval.Ticks, backedOff);

				// Unless that's shorter than the base interval, in which case we'll just apply the period
				var actual = Math.Max(period.Ticks, cappedBackoff);

				return TimeSpan.FromTicks(actual);
			}
		}
	}
}
