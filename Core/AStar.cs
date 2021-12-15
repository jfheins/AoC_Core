using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

using C5;

using SCG = System.Collections.Generic;

namespace Core
{
    public class AStarSearch<TNode> where TNode : notnull
    {
        public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

        private readonly SCG.IEqualityComparer<TNode> _comparer;
        private readonly Func<TNode, SCG.IEnumerable<(TNode node, float cost)>> _expander;

        private static readonly TimeSpan progressInterval = TimeSpan.FromMilliseconds(400);
        private AStarSearch<TNode>.ProgressReporterCallback _progressCallback;
        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        ///     Prepares a AStar search.
        /// </summary>
        /// <param name="expander">Callback to get the possible nodes from a source node</param>
        public AStarSearch(SCG.IEqualityComparer<TNode>? comparer,
                              Func<TNode, SCG.IEnumerable<(TNode node, float edgeCost)>> expander)
        {
            _comparer = comparer ?? EqualityComparer<TNode>.Default;
            _expander = expander;
            _progressCallback = //(_, _) => { };
            (set, visited) => Console.WriteLine($"A* visited {visited} nodes, working on {set}.");
        }

        public AStarPath? FindFirst(TNode initialNode,
                                      Func<TNode, bool> targetPredicate,
                                      Func<TNode, float> heuristic,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll(initialNode, targetPredicate, heuristic, progressReporter, 1);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Searches and finds all target nodes in the domain. If the domain is infinite, minResults should be filled.
        /// </summary>
        /// <param name="initialNode">Starting node for the search</param>
        /// <param name="targetPredicate">Must return true for a desired target</param>
        /// <param name="heuristic">Must return an estimate of the remaining cost. May underestimate but not overestimate.</param>
        /// <param name="progressReporter">Called periodically for status updates</param>
        /// <param name="minResults">Search will be terminated if at least this number of targets has been found.</param>
        /// <returns></returns>
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

            _stopwatch.Start();
            _progressCallback = progressReporter ?? _progressCallback;

            var visitedNodes = new HashSet<TNode>(_comparer);
            var nodeQueue = new IntervalHeap<AStarNode>();
            var openSet = new SCG.Dictionary<TNode, IPriorityQueueHandle<AStarNode>>(_comparer);

            // Helper methods
            void QueueNewNode(AStarNode node)
            {
                IPriorityQueueHandle<AStarNode>? handle = null;
                _ = nodeQueue.Add(ref handle, node);
                openSet[node.Item] = handle;
            }
            AStarNode PopMinNode()
            {
                var nextNode = nodeQueue.DeleteMin();
                _ = openSet.Remove(nextNode.Item);
                return nextNode;
            }
            void QueueOrUpdateNode(AStarNode currentNode, (TNode target, float cost) edge)
            {
                var newCost = currentNode.Cost + edge.cost;
                if (openSet.TryGetValue(edge.target, out var existingHandle))
                {
                    if (nodeQueue.Find(existingHandle, out var existing) && newCost < existing.Cost)
                    {
                        var newNode = new AStarNode(edge.target, currentNode, edge.cost, existing.Remaining);
                        _ = nodeQueue.Replace(existingHandle, newNode);
                        openSet[newNode.Item] = existingHandle;
                    }
                }
                else
                {
                    QueueNewNode(new AStarNode(edge.target, currentNode, edge.cost, heuristic(edge.target)));
                }
            }

            var origin = new AStarNode(initialNode, heuristic(initialNode));

            QueueNewNode(origin);

            var results = new ArrayList<AStarPath>();

            while (nodeQueue.Count > 0)
            {
                InvokeProgress(nodeQueue.Count, visitedNodes.Count);

                var currentNode = PopMinNode();
                _ = visitedNodes.Add(currentNode.Item);

                if (targetPredicate(currentNode.Item))
                {
                    _ = results.Add(new AStarPath(currentNode));
                    if (results.Count >= minResults)
                    {
                        break;
                    }
                }

                var expanded = _expander(currentNode.Item)
                    .Where(step => !visitedNodes.Contains(step.node));

                foreach (var edge in expanded)
                {
                    QueueOrUpdateNode(currentNode, edge);
                }
            }

            return results;
        }

        private void InvokeProgress(int workingSetCount, int visitedCount)
        {
            if (_stopwatch.Elapsed > progressInterval)
            {
                _progressCallback(workingSetCount, visitedCount);
                _stopwatch.Restart();
            }
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

#pragma warning disable CA1036 // Override methods on comparable types
        public class AStarNode : IComparable<AStarNode>
        {
            internal AStarNode(TNode initial, float remainder)
            {
                Item = initial;
                Predecessor = null;
                Cost = 0f;
                Remaining = remainder;
            }

            internal AStarNode(TNode current, AStarNode predecessor, float edgeCost = 0, float remainder = 0)
            {
                Item = current;
                Predecessor = predecessor;
                Cost = predecessor.Cost + edgeCost;
                Remaining = remainder;
            }

            public float Cost { get; }
            public float Remaining { get; }

            public float OverallEstimate => Cost + Remaining;

            public TNode Item { get; }
            private AStarNode? Predecessor { get; }

            public int CompareTo(AStarNode? other)
            {
                if (other is null)
                    return 1;

                return OverallEstimate.CompareTo(other.OverallEstimate);
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