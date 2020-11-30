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
    public class CombinatoricsTests
    {
        [TestMethod]
        public void CombinationsWork()
        {
            var combinations = new Combinations<int>(new int[] { 1, 2, 3, 4, 5 }, 2).ToList();
            var seen = new HashSet<int[]>();

            Assert.AreEqual(10, combinations.Count);
            foreach (var item in combinations)
            {
                Assert.AreEqual(2, item.Count);
                Assert.IsTrue(seen.Add(item.ToArray()), $"{item} was already seen");
            }
        }


        [TestMethod]
        public void CombinationsWorkWithOne()
        {
            var combCount = new Combinations<int>(new int[] { 1, 2, 3, 4, 5 }, 1).Count;
            Assert.AreEqual(5, combCount);
        }

        [TestMethod]
        public void PermutationsWork()
        {
            var perm = new Permutations<int>(new int[] { 1, 2, 3 }).ToList();

            Assert.AreEqual(6, perm.Count);
            foreach (var item in perm)
            {
                Assert.AreEqual(6, item.Sum());
            }
        }

        [TestMethod]
        public void PermutationsWithRepetitionWork()
        {
            var set = new int[] { 1, 2, 3, 3 };

            var perm1 = new Permutations<int>(set, GenerateOption.WithoutRepetition).ToList();
            var perm2 = new Permutations<int>(set, GenerateOption.WithRepetition).ToList();

            Assert.AreEqual(12, perm1.Count);
            Assert.AreEqual(24, perm2.Count);
        }
    }
}
