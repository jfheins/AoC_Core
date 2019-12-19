using System;
using System.Collections.Generic;

namespace Core
{
    public class BinarySearch
    {
        private readonly Func<long, bool> _predicate;

        public BinarySearch(Func<long, bool> predicate)
        {
            _predicate = predicate;
        }

        public long FindFirst(long start = 0)
        {
            if (_predicate(start))
                return start;

            var stepsize = 2;
            while (!_predicate(start + stepsize))
                stepsize *= 2;

            return FindFirst(start + (stepsize / 2), start + stepsize);
        }

        // lower: known false, upper: known true
        private long FindFirst(long lowerLimit, long upperLimit)
        {
            while (upperLimit - lowerLimit > 1)
            {
                var probeindex = lowerLimit + (upperLimit - lowerLimit) / 2;
                if (_predicate(probeindex))
                    upperLimit = probeindex;
                else
                    lowerLimit = probeindex;
            }
            return upperLimit;
        }
    }
}
