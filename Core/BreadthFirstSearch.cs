using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Core
{
    public class BreadthFirstSearch<TNode> where TNode: notnull
    {
        public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

        private readonly NodeComparer _comparer;
        private readonly Func<TNode, IEnumerable<TNode>> _expander;

        /// <summary>
        ///     Prepares a breadth first search.
        /// </summary>
        /// <param name="comparer">Comparison function that determines node equality</param>
        /// <param name="expander">Callback to get the possible nodes from a source node</param>
        public BreadthFirstSearch(IEqualityComparer<TNode>? comparer, Func<TNode, IEnumerable<TNode>> expander)
        {
            _comparer = new NodeComparer(comparer ?? EqualityComparer<TNode>.Default);
            _expander = expander;
        }

        public bool PerformParallelSearch { get; set; } = true;

        [CanBeNull]
        public IPath<TNode>? FindFirst(TNode initialNode,
                                      Func<TNode, bool> targetPredicate,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll(initialNode, targetPredicate, progressReporter, 1);
            return result.FirstOrDefault();
        }

        [CanBeNull]
        public IPath<TNode>? FindFirst(TNode initialNode,
                                      Func<NodeWithPredecessor, bool> targetPredicate,
                                      ProgressReporterCallback? progressReporter = null)
        {
            var result = FindAll2(initialNode, targetPredicate, progressReporter, 1);
            return result.FirstOrDefault();
        }

        [NotNull]
        public IList<IPath<TNode>> FindAll(TNode initialNode,
                                           Func<TNode, bool> targetPredicate,
                                           ProgressReporterCallback? progressReporter = null,
                                           int minResults = int.MaxValue)
        {
            return FindAll2(initialNode, bfsnode => targetPredicate(bfsnode.Item), progressReporter, minResults);
        }

        [NotNull]
        public IList<IPath<TNode>> FindAll2(TNode initialNode,
                                           Func<NodeWithPredecessor, bool> targetPredicate,
                                           ProgressReporterCallback? progressReporter = null,
                                           int minResults = int.MaxValue)
        {
            if (targetPredicate == null)
                throw new ArgumentNullException(nameof(targetPredicate), "A meaningful targetPredicate must be provided");

            var visitedNodes = new HashSet<NodeWithPredecessor>(_comparer);
            var initial = new NodeWithPredecessor(initialNode);
            var nextNodes = new HashSet<NodeWithPredecessor>(_comparer) { initial };
            var results = new List<IPath<TNode>>();

            var expander = PerformParallelSearch
                ? (Func<IEnumerable<NodeWithPredecessor>, IEnumerable<NodeWithPredecessor>>)(n =>
                   ParallelExpand(n, visitedNodes))
                : n => SequentialExpand(n, visitedNodes);

            if (targetPredicate(initial))
            {
                results.Add(new BfsPath(initialNode));
            }


            while (nextNodes.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nextNodes.Count);

                visitedNodes.UnionWith(nextNodes);

                var expanded = expander(nextNodes);
                nextNodes = new HashSet<NodeWithPredecessor>(expanded, _comparer);

                foreach (var node in nextNodes)
                {
                    if (targetPredicate(node))
                    {
                        results.Add(new BfsPath(node));
                    }
                }

                if (results.Count >= minResults)
                {
                    break;
                }
            }

            return results;
        }


        public IList<IPath<TNode>> FindLeafs(TNode initialNode,
                                           ProgressReporterCallback? progressReporter = null,
                                           int minResults = int.MaxValue)
        {
            var visitedNodes = new HashSet<NodeWithPredecessor>(_comparer);
            var initial = new NodeWithPredecessor(initialNode);
            var nextNodes = new HashSet<NodeWithPredecessor>(_comparer) { initial };
            var results = new List<IPath<TNode>>();

            while (nextNodes.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nextNodes.Count);
                visitedNodes.UnionWith(nextNodes);

                var expanded = SequentialExpandTuple(nextNodes, visitedNodes).ToList();

                nextNodes.Clear();
                foreach (var (pred, nodes) in expanded)
                {
                    var successorNodes = nodes.ToList();
                    if (successorNodes.Any())
                        nextNodes.UnionWith(successorNodes);
                    else
                        results.Add(new BfsPath(pred));
                }

                if (results.Count >= minResults)
                {
                    break;
                }
            }

            return results;
        }
        public IReadOnlyList<TNode> FindReachable(TNode initialNode,
                                 ProgressReporterCallback? progressReporter = null)
        {
            var visitedNodes = new HashSet<NodeWithPredecessor>(_comparer);
            var initial = new NodeWithPredecessor(initialNode);
            var nextNodes = new HashSet<NodeWithPredecessor>(_comparer) { initial };

            while (nextNodes.Count > 0)
            {
                progressReporter?.Invoke(visitedNodes.Count, nextNodes.Count);
                visitedNodes.UnionWith(nextNodes);

                var expanded = SequentialExpandTuple(nextNodes, visitedNodes).ToList();

                nextNodes.Clear();
                foreach (var (pred, nodes) in expanded)
                {
                    var successorNodes = nodes.ToList();
                    if (successorNodes.Any())
                        nextNodes.UnionWith(successorNodes);
                }
            }

            return visitedNodes.Select(n => n.Item).ToList();
        }


        private IEnumerable<NodeWithPredecessor> ParallelExpand(IEnumerable<NodeWithPredecessor> nextNodes,
                                                        HashSet<NodeWithPredecessor> visitedNodes)
        {
            return nextNodes.AsParallel()
                .SelectMany(sourceNode => _expander(sourceNode.Item)
                    .Select(dest => new NodeWithPredecessor(dest, sourceNode))
                    .Where(dest => !visitedNodes.Contains(dest)));
        }

        private IEnumerable<NodeWithPredecessor> SequentialExpand(IEnumerable<NodeWithPredecessor> nextNodes,
                                                                  HashSet<NodeWithPredecessor> visitedNodes)
        {
            return nextNodes
                .SelectMany(sourceNode => _expander(sourceNode.Item)
                    .Select(dest => new NodeWithPredecessor(dest, sourceNode))
                    .Where(dest => !visitedNodes.Contains(dest)));
        }

        private IEnumerable<(NodeWithPredecessor pred, IEnumerable<NodeWithPredecessor> nodes)> SequentialExpandTuple
            (IEnumerable<NodeWithPredecessor> nextNodes, HashSet<NodeWithPredecessor> visitedNodes)
        {
            return nextNodes
                .Select(sourceNode => (sourceNode, _expander(sourceNode.Item)
                    .Select(dest => new NodeWithPredecessor(dest, sourceNode))
                    .Where(node => !visitedNodes.Contains(node))));
        }

        private class BfsPath : IPath<TNode>
        {
            public BfsPath(TNode singleNode)
            {
                Target = singleNode;
                Length = 0;
                Steps = new[] { singleNode };
            }

            public BfsPath(NodeWithPredecessor target)
            {
                Target = target.Item;
                Steps = target.GetHistory().Reverse().ToArray();
                Length = Steps.Length - 1;
            }

            public TNode Target { get; }
            public int Length { get; }
            public TNode[] Steps { get; }
        }

        private class NodeComparer : EqualityComparer<NodeWithPredecessor>
        {
            public readonly IEqualityComparer<TNode> _comparer;

            public NodeComparer(IEqualityComparer<TNode> comparer)
            {
                _comparer = comparer;
            }

            public override bool Equals(NodeWithPredecessor? a, NodeWithPredecessor? b)
            {
                if (a is null || b is null)
                    return ReferenceEquals(a, b);

                return _comparer.Equals(a.Item, b.Item);
            }

            public override int GetHashCode(NodeWithPredecessor x)
            {
                return _comparer.GetHashCode(x.Item);
            }
        }

        public class NodeWithPredecessor
        {
            public NodeWithPredecessor(TNode current, NodeWithPredecessor? predecessor = null)
            {
                Predecessor = predecessor;
                Item = current;
                Distance = (predecessor?.Distance + 1) ?? 0;
            }

            public TNode Item { get; }
            public NodeWithPredecessor? Predecessor { get; }
            public int Distance { get; set; }

            public IEnumerable<TNode> GetHistory()
            {
                NodeWithPredecessor? pointer = this;
                do
                {
                    yield return pointer.Item;
                    pointer = pointer.Predecessor;
                } while (pointer != null);
            }
        }
    }
}