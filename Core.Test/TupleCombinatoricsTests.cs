using Microsoft.VisualStudio.TestTools.UnitTesting;

using Core;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Combinatorics;

namespace Core.Test
{
    [TestClass]
    public class TupleCombinatoricsTests
    {
        [TestMethod]
        public void CombinationsDoNotAllocate()
        {
            var combinations = new FastCombinations<int>(new int[] { 1, 2, 3, 4, 5 }, 2).ToList();
            Assert.IsTrue(ReferenceEquals(combinations[0], combinations[1]));
        }

        [TestMethod]
        public void CombinationsWork()
        {
            var combinations = new FastCombinations<int>(new int[] { 1, 2, 3, 4, 5 }, 2)
                .Select(x => x.ToArray()).ToList();

            var seen = new HashSet<int[]>();

            Assert.AreEqual(10, combinations.Count);
            foreach (var item in combinations)
            {
                Assert.AreEqual(2, item.Length);
                Assert.IsTrue(seen.Add(item.ToArray()), $"{item} was already seen");
            }
        }
    }
}
