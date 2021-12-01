using Microsoft.VisualStudio.TestTools.UnitTesting;

using Core;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Test
{
    [TestClass]
    public class LinqTests
    {
        [DataTestMethod]
        [DataRow("AABBCCDD", "AA", "BB", "CC", "DD")]
        [DataRow("AAAABBC", "AAAA", "BB", "C")]
        [DataRow("ABGR", "A", "B", "G", "R")]
        [DataRow("AAAAAAA", "AAAAAAA")]
        [DataRow("1100111", "11", "00", "111")]
        public void RightChunks(string data, params string[] expectedChunks)
        {
            var chunks = data.Chunks().Select(c => string.Concat(c)).ToList();
            CollectionAssert.AreEqual(expectedChunks, chunks, $"string: '{data}'");
        }

        [DataTestMethod]
        [DataRow("AABBCCDD")]
        [DataRow("AAAABBC")]
        [DataRow("ABGR")]
        [DataRow("AAAAAAA")]
        [DataRow("1100111")]
        public void RightRuns(string data)
        {
            var runs = data.Runs().ToList();
            var chunks = data.Chunks().ToList();

            foreach (var (run, chunk) in runs.Zip(chunks))
            {
                Assert.AreEqual(chunk.Length, run.Count);
                Assert.AreEqual(chunk.ToArray()[0], run.Element);
            }
        }

        [TestMethod]
        public void RightChunksNonString()
        {
            var array = new int[] { 1, 1, 1, 2, 2, 3, 6, 9, 9, 9, 8, 7, 7, 7, 5, 5, 4, 4, 4, 8, 8, 8, 8, 3, 3, 3, 3, 3, 9 };
            var chunks = array.Chunks();

            var expected = new int[][] {
                new int[] { 1, 1, 1 }, new int[] { 2, 2 }, new int[] { 3 }, new int[] { 6 },
                new int[] { 9, 9, 9 }, new int[] { 8 }, new int[] { 7, 7, 7 }, new int[] { 5, 5 },
                new int[] { 4, 4, 4 }, new int[] { 8, 8, 8, 8 }, new int[] { 3, 3, 3, 3, 3 }, new int[] {9 } };

            foreach (var (exp, result) in expected.Zip(chunks))
                CollectionAssert.AreEqual(exp, result);
        }


        [TestMethod]
        public void RightNumberOfDoubles()
        {
            static bool Check(int num) => num.ToString().Chunks().Any(c => c.Length == 2);

            var range = Enumerable.Range(134564, 450596);
            Assert.AreEqual(166392, range.Count(Check));
        }


        [TestMethod]
        public void StepBy3()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var result = array.StepBy(3).ToArray();
            CollectionAssert.AreEqual(new int[] {1, 4, 7} , result);
            result = array.StepBy(3, 1).ToArray();
            CollectionAssert.AreEqual(new int[] { 2, 5, 8 }, result);
            result = array.StepBy(4, 3).ToArray();
            CollectionAssert.AreEqual(new int[] { 4, 8 }, result);
            result = array.StepBy(1, 7).ToArray();
            CollectionAssert.AreEqual(new int[] { 8, 9 }, result);
        }
    }
}
