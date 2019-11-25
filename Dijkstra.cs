using System;
using System.Linq;
using C5;
using JetBrains.Annotations;
using SCG = System.Collections.Generic;

namespace Core
{
    public class DijkstraSearch<TNode, TEdge>
    {
        public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

        private readonly NodeComparer _comparer;

        private readonly Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> _expander;

        /// <summary>
        ///     Prepares a Dijkstra search.
        /// </summary>
        /// <param name="expander">Callback to get the possible edges</param>
        /// <param name="combiner">Callback to combine a source node and an edge to a (possibly new) node. May return null.</param>
        public DijkstraSearch(SCG.IEqualityComparer<TNode> comparer,
                              Func<TNode, SCG.IEnumerable<(TEdge edge, float cost)>> expander,
                              Func<TNode, TEdge, TNode> combiner)
        {
            _comparer = new NodeComparer(comparer);
            _expander = node =>
                expander(node).Select(weightedEdge => (combiner(node, weightedEdge.edge), weightedEdge.cost))
                    .Where(x => x.Item1 != null);
        }

        /// <summary>
        ///     Prepares a Dijkstra search.
        /// </summary>
        /// <param name="expander">Callback to get the possible nodes from a source node</param>
        public DijkstraSearch(SCG.IEqualityComparer<TNode> comparer,
                              Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> expander)
        {
            _comparer = new NodeComparer(comparer);
            _expander = expander;
        }

        [CanBeNull]
        public DijkstraPath? FindFirst(TNode initialNode,
                                      Func<TNode, bool> targetPredicate,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll(initialNode, targetPredicate, progressReporter, 1);
            return result.FirstOrDefault();
        }

        [NotNull]
        public SCG.IList<DijkstraPath> FindAll(TNode initialNode,
                                               Func<TNode, bool> targetPredicate,
                                               ProgressReporterCallback? progressReporter =
                                                   null,
                                               int minResults = int.MaxValue)
        {
            var initial = (initialNode, 0f).ToEnumerable();
            return FindAll(initial, targetPredicate, progressReporter, minResults);
        }

        [NotNull]
        public SCG.IList<DijkstraPath> FindAll(SCG.IEnumerable<(TNode node, float cost)> initial,
                                               Func<TNode, bool> targetPredicate,
                                               ProgressReporterCallback? progressReporter =
                                                   null,
                                               int minResults = int.MaxValue)
        {
            var visitedNodes = new HashSet<DijkstraNode>(_comparer);
            var nodeQueue = new IntervalHeap<DijkstraNode>();

            var initialNodes = initial.ToList();
            var origin = new DijkstraNode(initialNodes.Single(t => t.cost == 0).node);

            if (initialNodes.Count > 1)
            {
                _ = visitedNodes.Add(origin);
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
                IPriorityQueueHandle<DijkstraNode> handle = null;
                _ = nodeQueue.Add(ref handle, origin);
                origin.Handle = handle;
            }


            var results = new ArrayList<DijkstraPath>();

            while (nodeQueue.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nodeQueue.Count);

                var nextNode = nodeQueue.DeleteMin();
                visitedNodes.Add(nextNode);

                if (targetPredicate(nextNode.Current))
                {
                    results.Add(new DijkstraPath(nextNode));

                    if (results.Count >= minResults)
                    {
                        break;
                    }
                }

                var expanded = _expander(nextNode.Current)
                    .Select(dest => new DijkstraNode(dest.node, nextNode, dest.cost))
                    .Where(dest => !visitedNodes.Contains(dest))
                    .ToList();

                // Remove duplicated, favor the lower cost one
                foreach (var newNode in expanded)
                {
                    if (nodeQueue.Find(node => _comparer.Equals(node, newNode), out var existing))
                    {
                        if (existing.Cost > newNode.Cost)
                        {
                            nodeQueue.Replace(existing.Handle, newNode);
                        }
                    }
                    else
                    {
                        IPriorityQueueHandle<DijkstraNode> handle = null;
                        nodeQueue.Add(ref handle, newNode);
                        newNode.Handle = handle;
                    }
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
                Target = target.Current;
                Steps = target.GetHistory().Reverse().ToArray();
                Length = Steps.Length - 1;
                Cost = target.Cost;
            }

            public TNode Target { get; }
            public int Length { get; }
            public TNode[] Steps { get; }
            public float Cost { get; set; }
        }

        private class NodeComparer : SCG.EqualityComparer<DijkstraNode>
        {
            private readonly SCG.IEqualityComparer<TNode> _comparer;

            public NodeComparer(SCG.IEqualityComparer<TNode> comparer)
            {
                _comparer = comparer;
            }

            public override bool Equals(DijkstraNode a, DijkstraNode b)
            {
                return _comparer.Equals(a.Current, b.Current);
            }

            public override int GetHashCode(DijkstraNode x)
            {
                return _comparer.GetHashCode(x.Current);
            }
        }

        public class DijkstraNode : IComparable<DijkstraNode>
        {
            public DijkstraNode(TNode initial)
            {
                Current = initial;
                Predecessor = null;
                Cost = 0f;
            }

            public DijkstraNode(TNode current, DijkstraNode predecessor, float edgeCost = 0)
            {
                Current = current;
                Predecessor = predecessor;
                Cost = predecessor.Cost + edgeCost;
            }

            public float Cost { get; }
            public TNode Current { get; }
            private DijkstraNode Predecessor { get; }
            public IPriorityQueueHandle<DijkstraNode> Handle { get; set; }

            public int CompareTo(DijkstraNode other)
            {
                return Cost.CompareTo(other.Cost);
            }

            public SCG.IEnumerable<TNode> GetHistory()
            {
                var pointer = this;
                do
                {
                    yield return pointer.Current;
                    pointer = pointer.Predecessor;
                } while (pointer != null);
            }
        }
    }
}