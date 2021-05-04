using System;
using System.Collections.Generic;

namespace Serilog.Support
{
    public static class Randomize
    {
        private static readonly Random Random = new();

        public static T[] Values<T>(IEnumerable<T> values)
        {
            var randomizedValues = new List<T>(values);
            randomizedValues.Sort(new RandomizeComparer<T>(Random));
            return randomizedValues.ToArray();
        }

        private class RandomizeComparer<T> : IComparer<T>
        {
            private readonly Random random;

            public RandomizeComparer(Random random)
            {
                this.random = random;
            }

            public int Compare(T x, T y)
            {
                var z = random.Next(-1, 2);
                return z;
            }
        }
    }
}
