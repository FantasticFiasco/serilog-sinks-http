using System;

namespace Serilog.Sinks.Http.Private.Time
{
    public class ExponentialBackoffConnectionScheduleShould
    {
        //[Fact]
        public void Test()
        {
            var x = new ExponentialBackoffConnectionSchedule(TimeSpan.FromSeconds(2));

            while (true)
            {
                var y = x.NextInterval;

                x.MarkFailure();
            }
        }
    }
}
