using System;
using System.Diagnostics.Contracts;
using System.Linq;

using C5;

using SCG = System.Collections.Generic;

namespace Core
{
    public class DijkstraSearch<TNode> where TNode : notnull
    {
        public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

        private readonly SCG.IEqualityComparer<TNode> _comparer;
        private readonly Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> _expander;

        /// <summary>
        ///     Prepares a Dijkstra search.
        /// </summary>
        /// <param name="expander">Callback to get the possible nodes from a source node</param>
        public DijkstraSearch(SCG.IEqualityComparer<TNode>? comparer,
                              Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> expander)
        {
            _comparer = comparer ?? EqualityComparer<TNode>.Default;
            _expander = expander;
        }

        public DijkstraPath? FindFirst(TNode initialNode,
                                      Func<TNode, bool> targetPredicate,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll(initialNode, targetPredicate, progressReporter, 1);
            return result.FirstOrDefault();
        }

        public SCG.IList<DijkstraPath> FindAll(TNode initialNode,
                                               Func<TNode, bool> targetPredicate,
                                               ProgressReporterCallback? progressReporter =
                                                   null,
                                               int minResults = int.MaxValue)
        {
            var initial = (initialNode, 0f).ToEnumerable();
            return FindAll(initial, targetPredicate, progressReporter, minResults);
        }


        public SCG.IList<DijkstraPath> FindAll(SCG.IEnumerable<(TNode node, float cost)> initial,
                                               Func<TNode, bool> targetPredicate,
                                               ProgressReporterCallback? progressReporter =
                                                   null,
                                               int minResults = int.MaxValue)
        {
            var visitedNodes = new HashSet<TNode>(_comparer);
            var nodeQueue = new IntervalHeap<DijkstraNode>();

            var initialNodes = initial.ToList();
            var origin = new DijkstraNode(initialNodes.Single(t => t.cost == 0).node);

            if (initialNodes.Count > 1)
            {
                _ = visitedNodes.Add(origin.Item);
                foreach (var (node, cost) in initialNodes.Where(t => t.cost > 0))
                {
                    var dijkstraNode = new DijkstraNode(node, origin, cost);
                    IPriorityQueueHandle<DijkstraNode>? handle = null;
                    _ = nodeQueue.Add(ref handle, dijkstraNode);
                    dijkstraNode.Handle = handle;
                }
            }
            else
            {
                IPriorityQueueHandle<DijkstraNode>? handle = null;
                _ = nodeQueue.Add(ref handle, origin);
                origin.Handle = handle;
            }


            var results = new ArrayList<DijkstraPath>();

            while (nodeQueue.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nodeQueue.Count);

                DijkstraNode nextNode;
                do
                {
                    // The queue could contain visited nodes because deduping on insert is slow
                    nextNode = nodeQueue.DeleteMin();
                } while (visitedNodes.Contains(nextNode.Item));

                _ = visitedNodes.Add(nextNode.Item);

                if (targetPredicate(nextNode.Item))
                {
                    _ = results.Add(new DijkstraPath(nextNode));
                    if (results.Count >= minResults)
                    {
                        break;
                    }
                }

                var expanded = _expander(nextNode.Item)
                    .Where(step => !visitedNodes.Contains(step.node))
                    .Select(step => new DijkstraNode(step.node, nextNode, step.cost))
                    .ToList();

                foreach (var newNode in expanded)
                {
                    IPriorityQueueHandle<DijkstraNode>? handle = null;
                    _ = nodeQueue.Add(ref handle, newNode);
                    newNode.Handle = handle;
                }
            }

            return results;
        }

        public class DijkstraPath : IPath<TNode>
        {
            public DijkstraPath(TNode singleNode)
            {
                Target = singleNode;
                Length = 0;
                Steps = new[] { singleNode };
            }

            public DijkstraPath(DijkstraNode target)
            {
                Contract.Assert(target != null);
                Target = target.Item;
                Steps = target.GetHistory().Reverse().ToArray();
                Length = Steps.Length - 1;
                Cost = target.Cost;
            }

            public TNode Target { get; }
            public int Length { get; }
            public TNode[] Steps { get; }
            public float Cost { get; set; }
        }

        public class DijkstraNode : IComparable<DijkstraNode>
        {
            internal DijkstraNode(TNode item)
            {
                Item = item;
                Predecessor = null;
                Cost = 0f;
            }

            internal DijkstraNode(TNode item, DijkstraNode predecessor, float edgeCost = 0)
            {
                Item = item;
                Predecessor = predecessor;
                Cost = predecessor.Cost + edgeCost;
            }

            public float Cost { get; }
            public TNode Item { get; }
            private DijkstraNode? Predecessor { get; }
            public IPriorityQueueHandle<DijkstraNode>? Handle { get; set; }

            public int CompareTo(DijkstraNode? other)
            {
                if (other is null)
                    return 1;
                return Cost.CompareTo(other.Cost);
            }

            public SCG.IEnumerable<TNode> GetHistory()
            {
                var pointer = this;
                do
                {
                    yield return pointer.Item;
                    pointer = pointer.Predecessor;
                } while (pointer != null);
            }
        }
    }
}