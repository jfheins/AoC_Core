using System;

namespace Core
{
    public class BinarySearchInt
    {
        private readonly Func<int, bool> _predicate;

        /// <summary>
        /// Searches an intercal via binary search.
        /// The class offers methods to find the first value or the last value still matching.
        /// </summary>
        /// <param name="predicate"></param>
        public BinarySearchInt(Func<int, bool> predicate)
        {
            _predicate = predicate;
        }

        public int FindFirst(int start = 0) => SearchOpenInterval(start, true).upper;
        public int FindFirst(int start, int end) => SearchInterval(start, end, true).upper;
        public int FindLast(int start = 0) => SearchOpenInterval(start, false).lower;
        public int FindLast(int start, int end) => SearchInterval(start, end, false).lower;

        private (int lower, int upper) SearchOpenInterval(int lowerLimit, bool searchValue)
        {
            if (_predicate(lowerLimit) == searchValue)
                throw new ArgumentOutOfRangeException(nameof(lowerLimit), "The lowerLimit should not fulfil the seach condition!");

            var stepsize = 2;
            while (_predicate(lowerLimit + stepsize) != searchValue)
                stepsize *= 2;

            return SearchInterval(lowerLimit + (stepsize / 2), lowerLimit + stepsize, searchValue);
        }

        // lower: known false, upper: known true
        private (int lower, int upper) SearchInterval(int lowerLimit, int upperLimit, bool searchValue)
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
