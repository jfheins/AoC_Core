using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Core
{
    public static class Graph
    {
        public static Graph<TNode, TEdge> FromEdges<TNode, TEdge>(IEnumerable<TEdge> edges, Func<TEdge, (TNode, TNode)> linker) where TNode : notnull where TEdge : notnull
        {
            Contract.Assert(edges != null);

            var graphEdges = new List<GraphEdge<TNode, TEdge>>();

            var nodes = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
            var nodeFactory = new Func<TNode, GraphNode<TNode, TEdge>>(x => nodes.GetOrAdd(x, _ => new GraphNode<TNode, TEdge>(x)));

            foreach (var edge in edges)
            {
                var (src, dest) = linker(edge);
                var source = nodeFactory(src);
                var destination = nodeFactory(dest);

                var newEdge = new GraphEdge<TNode, TEdge>(edge, source, destination);
                graphEdges.Add(newEdge);
                source.OutgoingEdges.Add(newEdge);
                destination.IncomingEdges.Add(newEdge);
            }

            return new Graph<TNode, TEdge>(nodes.Values, graphEdges);
        }
    }

    public class Graph<TNode, TEdge> where TNode : notnull where TEdge : notnull
    {
        public IReadOnlyDictionary<TNode, GraphNode<TNode, TEdge>> Nodes { get; private set; }

        public IReadOnlyDictionary<TEdge, GraphEdge<TNode, TEdge>> Edges { get; private set; }

        public NodeComparer<TNode, TEdge> NodeComparer { get; set; }

        //public IEnumerable<GraphNode<TNode, TEdge>> Sources { get; set; }
        //public IEnumerable<GraphNode<TNode, TEdge>> Sinks { get; set; }

        internal Graph(ICollection<GraphNode<TNode, TEdge>> nodes, ICollection<GraphEdge<TNode, TEdge>> edges)
        {
            Nodes = nodes?.ToDictionary(n => n.Value) ?? throw new ArgumentNullException(nameof(nodes));
            Edges = edges?.ToDictionary(e => e.Value) ?? throw new ArgumentNullException(nameof(edges));

            NodeComparer = new NodeComparer<TNode, TEdge>(EqualityComparer<TNode>.Default);
        }
    }

    [DebuggerDisplay("Node <{Value}>")]
    public sealed class GraphNode<TNode, TEdge>
    {
        public GraphNode(TNode value)
        {
            Value = value;
        }

        public ICollection<GraphEdge<TNode, TEdge>> IncomingEdges { get; } = new List<GraphEdge<TNode, TEdge>>();

        public ICollection<GraphEdge<TNode, TEdge>> OutgoingEdges { get; } = new List<GraphEdge<TNode, TEdge>>();

        public TNode Value { get; set; }
        public IEnumerable<GraphNode<TNode, TEdge>> Neighbors => IncomingEdges.Select(e => e.Source).Concat(OutgoingEdges.Select(e => e.Destination));
    }

    [DebuggerDisplay("Edge <{Value}>")]
    public sealed class GraphEdge<TNode, TEdge>
    {
        public GraphEdge(TEdge value, GraphNode<TNode, TEdge> source, GraphNode<TNode, TEdge> destination)
        {
            Value = value;
            Source = source;
            Destination = destination;
        }

        public GraphNode<TNode, TEdge> Source { get; }

        public GraphNode<TNode, TEdge> Destination { get; }

        public TEdge Value { get; set; }
    }

    public class NodeComparer<TNode, TEdge> : EqualityComparer<GraphNode<TNode, TEdge>> where TNode : notnull
    {
        private readonly IEqualityComparer<TNode> _comparer;

        public NodeComparer(IEqualityComparer<TNode> comparer)
        {
            _comparer = comparer;
        }

        public override bool Equals(GraphNode<TNode, TEdge>? a, GraphNode<TNode, TEdge>? b)
        {
            if (a is null || b is null)
                return ReferenceEquals(a, b);

            return _comparer.Equals(a.Value, b.Value);
        }

        public override int GetHashCode(GraphNode<TNode, TEdge> x)
        {
            Contract.Assert(x != null);
            return _comparer.GetHashCode(x.Value);
        }
    }
}
