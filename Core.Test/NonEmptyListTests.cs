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
    public class NonEmptyListTests
    {
        [TestMethod]
        public void CanBeCreated()
        {
            var list = new NonEmptyList<int>(1);
            Assert.AreEqual(1, list.Head);
        }


        [TestMethod]
        public void ThrowsWhenRemovingLastElement()
        {
            var list = new NonEmptyList<int>(1) { 2, 3, 4 };
            Assert.AreEqual(4, list.Count);
            _ = list.Remove(2);
            Assert.IsFalse(list.Contains(2));
            list.RemoveAt(0);
            Assert.AreEqual(3, list.Head);
            list.RemoveAt(1);
            Assert.AreEqual(0, list.IndexOf(3));

            _ = Assert.ThrowsException<NotSupportedException>(() => list.Clear());
            _ = Assert.ThrowsException<NotSupportedException>(() => list.RemoveAt(0));
        }
    }
}
