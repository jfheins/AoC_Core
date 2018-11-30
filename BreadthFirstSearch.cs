using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using JetBrains.Annotations;

namespace Core
{
	public class BreadthFirstSearch<TNode, TEdge>
	{
		public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

		private readonly IEqualityComparer<NodeWithPredecessor> _comparer;
		private readonly Func<TNode, IEnumerable<TNode>> _expander;

		/// <summary>
		///     Prepares a breadth first search.
		/// </summary>
		/// <param name="comparer">Comparison function that determines node equality</param>
		/// <param name="expander">Callback to get the possible edges</param>
		/// <param name="combiner">Callback to combine a source node and an edge to a (possibly new) node. May return null.</param>
		public BreadthFirstSearch(IEqualityComparer<TNode> comparer,
			Func<TNode, IEnumerable<TEdge>> expander,
			Func<TNode, TEdge, TNode> combiner)
		{
			_comparer = new NodeComparer(comparer);
			_expander = node => expander(node).Select(edge => combiner(node, edge)).Where(x => x != null);
		}

		/// <summary>
		///     Prepares a breadth first search.
		/// </summary>
		/// <param name="comparer">Comparison function that determines node equality</param>
		/// <param name="expander">Callback to get the possible nodes from a source node</param>
		public BreadthFirstSearch(IEqualityComparer<TNode> comparer, Func<TNode, IEnumerable<TNode>> expander)
		{
			_comparer = new NodeComparer(comparer);
			_expander = expander;
		}

        [CanBeNull]
		public IPath<TNode, TEdge> FindFirst(TNode initialNode,
			Func<TNode, bool> targetPredicate,
			ProgressReporterCallback progressReporter = null)
		{
			var result = FindAll(initialNode, targetPredicate, progressReporter, 1);
			return result.FirstOrDefault();
		}

        [NotNull]
		public IList<IPath<TNode, TEdge>> FindAll(TNode initialNode,
			Func<TNode, bool> targetPredicate,
			ProgressReporterCallback progressReporter = null,
			int minResults = int.MaxValue)
		{
			var visitedNodes = new HashSet<NodeWithPredecessor>(_comparer);
			var nextNodes = new HashSet<NodeWithPredecessor>(_comparer) { new NodeWithPredecessor(initialNode) };

			var results = new List<IPath<TNode, TEdge>>();

			if (targetPredicate(initialNode))
				results.Add(new BfsPath(initialNode));

			while (nextNodes.Count > 0)
			{
				progressReporter?.Invoke(visitedNodes.Count, nextNodes.Count);

				visitedNodes.UnionWith(nextNodes);

				var expanded = nextNodes.AsParallel()
					.SelectMany(sourceNode => _expander(sourceNode.Current)
						.Select(dest => new NodeWithPredecessor(dest, sourceNode))
						.Where(dest => !visitedNodes.Contains(dest)));

				nextNodes = new HashSet<NodeWithPredecessor>(expanded, _comparer);

				foreach (var node in nextNodes)
					if (targetPredicate(node.Current))
						results.Add(new BfsPath(node));

				if (results.Count >= minResults)
					break;
			}

			return results;
		}

		private class BfsPath : IPath<TNode, TEdge>
		{
			public TNode Target { get; }
			public int Length { get; }
			public TNode[] Steps { get; }

			public BfsPath(TNode singleNode)
			{
				Target = singleNode;
				Length = 0;
				Steps = new[] { singleNode };
			}

			public BfsPath(NodeWithPredecessor target)
			{
				Target = target.Current;
				Steps = target.GetHistory().Reverse().ToArray();
				Length = Steps.Length - 1;
			}
		}

		private class NodeComparer : EqualityComparer<NodeWithPredecessor>
		{
			private readonly IEqualityComparer<TNode> _comparer;

			public NodeComparer(IEqualityComparer<TNode> comparer)
			{
				_comparer = comparer;
			}

			public override bool Equals(NodeWithPredecessor a, NodeWithPredecessor b)
			{
				return _comparer.Equals(a.Current, b.Current);
			}

			public override int GetHashCode(NodeWithPredecessor x)
			{
				return _comparer.GetHashCode(x.Current);
			}
		}

		private class NodeWithPredecessor
		{
			public NodeWithPredecessor(TNode current, NodeWithPredecessor predecessor = null)
			{
				Predecessor = predecessor;
				Current = current;
			}

			public TNode Current { get; }
			private NodeWithPredecessor Predecessor { get; }

			public IEnumerable<TNode> GetHistory()
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