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
    public class PriorityDictionaryTests
    {
        [TestMethod]
        public void CanBeConstructed()
        {
            var dict = new PriorityDictionary<int, string>();
            Assert.IsTrue(dict is IDictionary<int, string>);
        }

        [TestMethod]
        public void CanAddItems()
        {
            var dict = new PriorityDictionary<int, string>();
            dict.Add(1, "Test");
            Assert.AreEqual(dict[1], "Test");
        }


        [TestMethod]
        public void CanRemoveMinValue()
        {
            var dict = new PriorityDictionary<int, string>();
            dict.Add(1, "BBB");
            dict.Add(2, "AAA");
            var min = dict.PopMin();
            Assert.AreEqual("AAA", min.Value);
            Assert.AreEqual(2, min.Key);
        }


        [TestMethod]
        public void CanPeekValues()
        {
            var dict = new PriorityDictionary<Point, float>();

            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 10; y++)
                    dict.Add(new Point(x, y), ((x - 2) * (x - 2.5f)) + ((y - 3) * (y - 3.2f)));
            }

            var min = dict.PopMin();
            Assert.AreEqual(0, min.Value, 0.01f);
            Assert.AreEqual(new Point(2, 3), min.Key);

            min = dict.PeekMin();
            Assert.AreEqual(0.5, min.Value, 0.01f);
            Assert.AreEqual(new Point(3, 3), min.Key);

            var max = dict.PeekMax();
            Assert.AreEqual(80.3f, max.Value, 0.01f);
            Assert.AreEqual(new Point(9, 9), max.Key);
        }


        private struct TestContainer
        {
            public int Content;
            public TestContainer(int v)
            {
                Content = v;
            }
        }
        private class TestContainerComparer : IComparer<TestContainer>
        {
            public int Compare(TestContainer x, TestContainer y) => x.Content.CompareTo(y.Content);
        }

        [TestMethod]
        public void ThrowsIfNotComparable()
        {
            var dict = new PriorityDictionary<Point, TestContainer>();
            _ = Assert.ThrowsException<ArgumentException>(() =>
              {
                  dict.Add(new Point(), new TestContainer());
                  dict.Add(new Point(), new TestContainer());
              });
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CanUseCustomComparer(bool useComparer)
        {
            var dict = new PriorityDictionary<Point, TestContainer>((a, b) => a.Content.CompareTo(b.Content));
            if (useComparer)
            {
                dict = new PriorityDictionary<Point, TestContainer>(new TestContainerComparer());
            }

            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < 10; y++)
                    dict.Add(new Point(x, y), new TestContainer(Math.Abs(x - 4) + Math.Abs(y - 6)));
            }

            var min = dict.PopMin();
            Assert.AreEqual(0, min.Value.Content);
            Assert.AreEqual(new Point(4, 6), min.Key);

            min = dict.PeekMin();
            Assert.AreEqual(1, min.Value.Content);

            var max = dict.PeekMax();
            Assert.AreEqual(11, max.Value.Content);
        }
    }
}
