using System;
using System.Linq;
using C5;
using SCG = System.Collections.Generic;

namespace Core
{
    public class AStarSearch<TNode> where TNode : notnull
    {
        public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

        private readonly NodeComparer _comparer;
        private readonly Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> _expander;

        /// <summary>
        ///     Prepares a AStar search.
        /// </summary>
        /// <param name="expander">Callback to get the possible nodes from a source node</param>
        public AStarSearch(SCG.IEqualityComparer<TNode> comparer,
                              Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> expander)
        {
            _comparer = new NodeComparer(comparer);
            _expander = expander;
        }

        public AStarPath? FindFirst(TNode initialNode,
                                      Func<TNode, bool> targetPredicate,
                                      Func<TNode, float> heuristic,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll(initialNode, targetPredicate, heuristic, progressReporter, 1);
            return result.FirstOrDefault();
        }

        public SCG.IList<AStarPath> FindAll(TNode initialNode,
                                               Func<TNode, bool> targetPredicate,
                                               Func<TNode, float> heuristic,
                                               ProgressReporterCallback? progressReporter =
                                                   null,
                                               int minResults = int.MaxValue)
        {
            if (heuristic == null)
            {
                throw new ArgumentNullException(nameof(heuristic));
            }

            var visitedNodes = new HashSet<AStarNode>(_comparer);
            var nodeQueue = new IntervalHeap<AStarNode>();
            var openSet = new SCG.Dictionary<TNode, AStarNode>();

            // Helper methods
            void QueueNewNode(AStarNode node)
            {
                IPriorityQueueHandle<AStarNode>? handle = null;
                _ = nodeQueue.Add(ref handle, node);
                node.Handle = handle;
                openSet[node.Current] = node;
            }
            AStarNode PopMinNode()
            {
                var nextNode = nodeQueue.DeleteMin();
                _ = openSet.Remove(nextNode.Current);
                return nextNode;
            }
            void QueueOrUpdateNode(AStarNode newNode)
            {
                if (openSet.TryGetValue(newNode.Current, out var existing))
                {
                    if (existing.Cost > newNode.Cost)
                    {
                        _ = nodeQueue.Replace(existing.Handle, newNode);
                        newNode.Handle = existing.Handle;
                        openSet[newNode.Current] = newNode;
                    }
                }
                else
                {
                    QueueNewNode(newNode);
                }
            }

            var origin = new AStarNode(initialNode, heuristic(initialNode));

            QueueNewNode(origin);

            var results = new ArrayList<AStarPath>();

            while (nodeQueue.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nodeQueue.Count);

                var currentNode = PopMinNode();
                _ = visitedNodes.Add(currentNode);

                if (targetPredicate(currentNode.Current))
                {
                    _ = results.Add(new AStarPath(currentNode));
                    if (results.Count >= minResults)
                    {
                        break;
                    }
                }

                var expanded = _expander(currentNode.Current)
                    .Select(step => new AStarNode(step.node, currentNode, step.cost, heuristic(step.node)))
                    .Where(node => !visitedNodes.Contains(node))
                    .ToList();

                foreach (var newNode in expanded)
                {
                    QueueOrUpdateNode(newNode);
                }
            }

            return results;
        }

        public class AStarPath : IPath<TNode>
        {
            public AStarPath(TNode singleNode)
            {
                Target = singleNode;
                Length = 0;
                Steps = new[] { singleNode };
            }

            public AStarPath(AStarNode target)
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

        private class NodeComparer : SCG.EqualityComparer<AStarNode>
        {
            public readonly SCG.IEqualityComparer<TNode> _comparer;

            public NodeComparer(SCG.IEqualityComparer<TNode> comparer)
            {
                _comparer = comparer;
            }

            public override bool Equals(AStarNode a, AStarNode b)
            {
                return _comparer.Equals(a.Current, b.Current);
            }

            public override int GetHashCode(AStarNode x)
            {
                return _comparer.GetHashCode(x.Current);
            }
        }

        public class AStarNode : IComparable<AStarNode>
        {
            internal AStarNode(TNode initial, float remainder)
            {
                Current = initial;
                Predecessor = null;
                Cost = 0f;
            }

            internal AStarNode(TNode current, AStarNode predecessor, float edgeCost = 0, float remainder = 0)
            {
                Current = current;
                Predecessor = predecessor;
                Cost = predecessor.Cost + edgeCost;
                Remaining = remainder;
            }

            public float Cost { get; }
            public float Remaining { get; }

            public float OverallEstimate => Cost + Remaining;

            public TNode Current { get; }
            private AStarNode Predecessor { get; }
            public IPriorityQueueHandle<AStarNode>? Handle { get; set; }

            public int CompareTo(AStarNode other) => OverallEstimate.CompareTo(other.OverallEstimate);

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