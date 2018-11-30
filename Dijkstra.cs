using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using JetBrains.Annotations;

namespace Core
{
	public class DijkstraSearch<TNode, TEdge>
	{
		public delegate void ProgressReporterCallback(int workingSetCount, int visitedCount);

		private readonly Func<TNode, IEnumerable<(TNode node, uint cost)>> _expander;

		/// <summary>
		///     Prepares a Dijkstra search.
		/// </summary>
		/// <param name="expander">Callback to get the possible edges</param>
		/// <param name="combiner">Callback to combine a source node and an edge to a (possibly new) node. May return null.</param>
		public DijkstraSearch(Func<TNode, IEnumerable<(TEdge edge, uint cost)>> expander,
							  Func<TNode, TEdge, TNode> combiner)
		{
			_expander = node =>
				expander(node).Select(weightedEdge => (combiner(node, weightedEdge.edge), weightedEdge.cost))
					.Where(x => x.Item1 != null);
		}

		/// <summary>
		///     Prepares a Dijkstra search.
		/// </summary>
		/// <param name="expander">Callback to get the possible nodes from a source node</param>
		public DijkstraSearch(Func<TNode, IEnumerable<(TNode node, uint cost)>> expander)
		{
			_expander = expander;
		}

		[CanBeNull]
		public IPath<TNode> FindFirst(TNode initialNode,
											 Func<TNode, bool> targetPredicate,
											 ProgressReporterCallback progressReporter = null)
		{
			var result = FindAll(initialNode, targetPredicate, progressReporter, 1);
			return result.FirstOrDefault();
		}

		[NotNull]
		public System.Collections.Generic.IList<IPath<TNode>> FindAll(TNode initialNode,
																			 Func<TNode, bool> targetPredicate,
																			 ProgressReporterCallback progressReporter =
																				 null,
																			 int minResults = int.MaxValue)
		{
			var visitedNodes = new System.Collections.Generic.HashSet<DijkstraNode>();
			var nodeQueue = new IntervalHeap<DijkstraNode>
			{
				new DijkstraNode(initialNode)
			};
			var nextNodes = new System.Collections.Generic.HashSet<DijkstraNode> {new DijkstraNode(initialNode)};

			var results = new List<IPath<TNode>>();

			if (targetPredicate(initialNode))
				results.Add(new DijkstraPath(initialNode));

			while (nodeQueue.Count > 0)
			{
				progressReporter?.Invoke(visitedNodes.Count, nextNodes.Count);
				visitedNodes.UnionWith(nextNodes);

				var nextNode = nodeQueue.DeleteMin();

				var expanded = _expander(nextNode.Current)
					.Select(dest => new DijkstraNode(dest.node, nextNode, dest.cost))
					.Where(dest => !visitedNodes.Contains(dest))
					.ToList();

				nodeQueue.AddAll(expanded);

				foreach (var node in expanded)
					if (targetPredicate(node.Current))
						results.Add(new DijkstraPath(node));

				if (results.Count >= minResults)
					break;
			}

			return results;
		}

		private class DijkstraPath : IPath<TNode>
		{
			public DijkstraPath(TNode singleNode)
			{
				Target = singleNode;
				Length = 0;
				Steps = new[] {singleNode};
			}

			public DijkstraPath(DijkstraNode target)
			{
				Target = target.Current;
				Steps = target.GetHistory().Reverse().ToArray();
				Length = Steps.Length - 1;
			}

			public TNode Target { get; }
			public int Length { get; }
			public TNode[] Steps { get; }
		}

		private class DijkstraNode : IComparable<DijkstraNode>
		{
			public DijkstraNode(TNode initial)
			{
				Current = initial;
				Predecessor = null;
				Cost = 0;
			}

			public DijkstraNode(TNode current, DijkstraNode predecessor, uint edgeCost = 0)
			{
				Current = current;
				Predecessor = predecessor;
				Cost = predecessor.Cost + edgeCost;
			}

			public uint Cost { get; }
			public TNode Current { get; }
			private DijkstraNode Predecessor { get; }

			public int CompareTo(DijkstraNode other)
			{
				return Cost.CompareTo(other.Cost);
			}

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