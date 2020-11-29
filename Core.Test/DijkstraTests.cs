using Microsoft.VisualStudio.TestTools.UnitTesting;

using Core;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Core.Test
{
    [TestClass]
    public class DijkstraTests
    {

        [TestMethod]
        public void FindsInitialNode()
        {
            var search = new DijkstraSearch<int>(
                EqualityComparer<int>.Default,
                _ => Enumerable.Empty<(int, float)>());

            var result = search.FindAll(33, x => true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result[0].Length);
            Assert.AreEqual(1, result[0].Steps.Length);
            Assert.AreEqual(33, result[0].Steps[0]);
        }

        [TestMethod]
        public void ReturnsEmptySetIfNothingFound()
        {
            var search = new DijkstraSearch<int>(EqualityComparer<int>.Default, _ => Enumerable.Empty<(int, float)>());
            var result = search.FindAll(0, x => false);

            Assert.AreEqual(0, result.Count);
        }

        private static IEnumerable<(int node, float cost)> InfiniteExpander(int x)
        {
            yield return (x * 2, 1);
            yield return (x * 3, 1);
            yield return (x * 6, 1.8f);
        }

        private static IEnumerable<(int node, float cost)> FiniteExpander(int x)
        {
            if (x < 100)
            {
                yield return (x * 2, 1);
                yield return (x * 3, 1);
                yield return (x * 6, 1.8f);
            }
        }

        [TestMethod]
        public void FindsFirstTarget()
        {
            var search = new DijkstraSearch<int>(EqualityComparer<int>.Default, InfiniteExpander);
            var result = search.FindFirst(1, x => x == 192);

            Assert.AreEqual(1, result.Steps.First());
            Assert.AreEqual(192, result.Steps.Last());
            Assert.AreEqual(6, result.Length);
            Assert.AreEqual(5 + 1.8, result.Cost, 1e-6f);
        }

        [TestMethod]
        public void FindTargetSetInInfiniteGraph()
        {
            var search = new DijkstraSearch<int>(EqualityComparer<int>.Default, InfiniteExpander);

            var result = search.FindAll(1, x => x < 30, null, 12);

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
            var search = new DijkstraSearch<int>(
                EqualityComparer<int>.Default,
                FiniteExpander);

            var result = search.FindAll(1, x => x < 30);

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
            var search = new DijkstraSearch<int>(
                EqualityComparer<int>.Default,
                FiniteExpander);

            var targets = new Dictionary<int, float> {
                { 6, 1.8f },
                { 12, 2.8f },
                { 24, 3.8f },
                { 36, 3.6f },
                { 72, 4.6f },
                { 108, 4.6f },
                { 192, 6.8f }
            };

            var result = search.FindAll(1, node => targets.ContainsKey(node));


            Assert.AreEqual(targets.Count, result.Count);
            foreach (var hit in result)
            {
                Assert.AreEqual(targets[hit.Target], hit.Cost, 1e-6f);
            }
        }
    }
}
