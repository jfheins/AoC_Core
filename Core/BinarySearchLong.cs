using System;
using System.Collections.Generic;

namespace Core
{
    public class BinarySearchLong
    {
        private readonly Func<long, bool> _predicate;

        /// <summary>
        /// Searches an intercal via binary search.
        /// The class offers methods to find the first value or the last value still matching.
        /// </summary>
        /// <param name="predicate"></param>
        public BinarySearchLong(Func<long, bool> predicate)
        {
            _predicate = predicate;
        }

        public long FindFirst(long start = 0) => SearchOpenInterval(start, true).upper;
        public long FindLast(long start = 0) => SearchOpenInterval(start, false).lower;

        private (long lower, long upper) SearchOpenInterval(long lowerLimit, bool searchValue)
        {
            if (_predicate(lowerLimit) == searchValue)
                throw new ArgumentOutOfRangeException(nameof(lowerLimit), "The lowerLimit should not fulfil the seach condition!");

            var stepsize = 2;
            while (_predicate(lowerLimit + stepsize) != searchValue)
                stepsize *= 2;

            return SearchInterval(lowerLimit + (stepsize / 2), lowerLimit + stepsize, searchValue);
        }

        // lower: known false, upper: known true
        private (long lower, long upper) SearchInterval(long lowerLimit, long upperLimit, bool searchValue)
        {
            while (upperLimit - lowerLimit > 1)
            {
                var probeindex = lowerLimit + (upperLimit - lowerLimit) / 2;
                if (_predicate(probeindex) == searchValue)
                    upperLimit = probeindex;
                else
                    lowerLimit = probeindex;
            }
            return (lowerLimit, upperLimit);
        }
    }
}
