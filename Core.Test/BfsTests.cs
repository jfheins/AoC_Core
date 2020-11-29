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
    public class BfsTests
    {
        [TestMethod]
        public void FindsInitialNode()
        {
            var bfs = new BreadthFirstSearch<int>(EqualityComparer<int>.Default, _ => Enumerable.Empty<int>());
            var result = bfs.FindAll(33, x => x > 0);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].Length);
            Assert.AreEqual(1, result[0].Steps.Length);
            Assert.AreEqual(33, result[0].Steps[0]);
        }

        [TestMethod]
        public void ReturnsEmptySetIfNothingFound()
        {
            var bfs = new BreadthFirstSearch<int>(EqualityComparer<int>.Default, _ => Enumerable.Empty<int>());
            var result = bfs.FindAll(0, x => x < 0);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void FindsFirstTarget()
        {
            var bfs = new BreadthFirstSearch<int>(EqualityComparer<int>.Default, x => new[] { x * 2, x * 3 });
            var result = bfs.FindFirst(1, x => x == 192);

            Assert.AreEqual(1, result.Steps.First());
            Assert.AreEqual(192, result.Steps.Last());
            Assert.AreEqual(7, result.Length);
        }

        [TestMethod]
        public void FindTargetSetInInfiniteGraph()
        {
            var bfs = new BreadthFirstSearch<int>(EqualityComparer<int>.Default, x => new[] { x * 2, x * 3 });

            var result = bfs.FindAll(1, x => x < 30, null, 12);

            var expected = new[] { 1, 2, 4, 8, 16, 3, 6, 12, 24, 9, 18, 27 };
            Assert.AreEqual(12, result.Count);
            foreach (var target in expected)
            {
                Assert.IsTrue(result.Any(p => p.Target == target));
            }
        }

        [TestMethod]
        public void FindAllTargetsInFiniteGraph()
        {
            var bfs = new BreadthFirstSearch<int>(
                EqualityComparer<int>.Default,
                (int x) => (x > 100) ? Enumerable.Empty<int>() : new[] { x * 2, x * 3 });

            var result = bfs.FindAll(1, x => x < 30);

            var expected = new[] { 1, 2, 4, 8, 16, 3, 6, 12, 24, 9, 18, 27 };
            Assert.AreEqual(12, result.Count);
            foreach (var target in expected)
            {
                Assert.IsTrue(result.Any(p => p.Target == target));
            }
        }

        [TestMethod]
        public void FindAllTargetsWithDistance()
        {
            var bfs = new BreadthFirstSearch<int>(
                EqualityComparer<int>.Default,
                (int x) => (x > 100) ? Enumerable.Empty<int>() : new[] { x * 2, x * 3 });

            var result = bfs.FindAll2(1, node => node.Distance > 4);

            Assert.AreEqual(13, result.Count);
        }

        [TestMethod]
        public void FindAllTargetsWithPredecessor()
        {
            var bfs = new BreadthFirstSearch<int>(EqualityComparer<int>.Default, x => new[] { x * 2, x * 3 });

            var result = bfs.FindAll2(1, node => node.Predecessor?.Item == 64, null, 1);

            Assert.IsTrue(result.Count == 1 || result.Count == 2);
            Assert.IsTrue(result.All(x => x.Target == 128 || x.Target == 192));
        }

        [TestMethod]
        public void SearchIsExhaustive()
        {
            var expansionCount = 0;

            IEnumerable<Point> Expander(Point p)
            {
                expansionCount++;
                if (p.X + p.Y >= 100)
                    yield break;

                yield return p + new Size(1, 0);
                yield return p + new Size(0, 1);
            }

            var bfs = new BreadthFirstSearch<Point>(EqualityComparer<Point>.Default, Expander)
            {
                PerformParallelSearch = false
            };

            var result = bfs.FindAll(Point.Empty, p => (p.X + p.Y) % 5 == 0);

            var resultCount = (100 / 5 * (100 + 5) / 2) + (100 / 5) + 1;
            Assert.AreEqual(resultCount, result.Count);
            Assert.AreEqual(5151, expansionCount);
        }

        [TestMethod]
        public void ParallelSearchIsExhaustive()
        {
            var expansionCount = 0;

            IEnumerable<Point> Expander(Point p)
            {
                _ = Interlocked.Increment(ref expansionCount);
                if (p.X + p.Y >= 100)
                    yield break;

                yield return p + new Size(1, 0);
                yield return p + new Size(0, 1);
            }

            var bfs = new BreadthFirstSearch<Point>(EqualityComparer<Point>.Default, Expander)
            {
                PerformParallelSearch = true
            };

            var result = bfs.FindAll(Point.Empty, p => (p.X + p.Y) % 5 == 0);

            var resultCount = (100 / 5 * (100 + 5) / 2) + (100 / 5) + 1;
            Assert.AreEqual(resultCount, result.Count);
            Assert.AreEqual(5151, expansionCount);
        }
    }
}
