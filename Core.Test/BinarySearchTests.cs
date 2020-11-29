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
    public class BinarySearchTests
    {
        [TestMethod]
        public void FindsFirstItemOpenInterval()
        {
            var result = new BinarySearchLong(x => x > 22).FindFirst();
            Assert.AreEqual(23, result);
        }

        [TestMethod]
        public void FindsLastOpenInterval()
        {
            var result = new BinarySearchLong(x => x < 27).FindLast();
            Assert.AreEqual(26, result);
        }

        [TestMethod]
        public void ChecksStart()
        {
            _ = Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BinarySearchLong(x => x < 27).FindFirst());
        }

        [TestMethod]
        public void SearchesEfficiently()
        {
            var callCount = 0;
            bool predicate(long x)
            {
                callCount++;
                return x > 65465;
            }
            var result = new BinarySearchLong(predicate).FindFirst();
            Assert.AreEqual(65466, result);
            Assert.AreEqual(32, callCount);
        }
    }
}
