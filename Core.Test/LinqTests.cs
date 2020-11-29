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


        [TestMethod]
        public void RightNumberOfDoubles()
        {
            static bool Check(int num) => num.ToString().ToCharArray().Chunks().Any(c => c.Count() == 2);

            var range = Enumerable.Range(134564, 450596);
            Assert.AreEqual(166392, range.Count(Check));
        }
    }
}
