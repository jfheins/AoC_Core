using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Graph<TNode, TEdge> where TNode : notnull
    {
        public ICollection<GraphNode<TNode, TEdge>> Nodes { get; private set; }

        public ICollection<GraphEdge<TNode, TEdge>> Edges { get; private set; }

        public IEnumerable<GraphNode<TNode, TEdge>> Sources { get; set; }
        public IEnumerable<GraphNode<TNode, TEdge>> Sinks { get; set; }

        public static Graph<TNode, TEdge> FromEdges(IEnumerable<TEdge> edges, Func<TEdge, (TNode, TNode)> linker)
        {
            var edgeValues = edges.ToList();
            var graphEdges = new List<GraphEdge<TNode, TEdge>>();

            var nodes = new Dictionary<TNode, GraphNode<TNode, TEdge>>();
            var nodeFactory = new Func<TNode, GraphNode<TNode, TEdge>>(x => nodes.GetOrAdd(x, _ => new GraphNode<TNode, TEdge>(x)));

            foreach (var edge in edgeValues)
            {
                var (src, dest) = linker(edge);

                var source = nodeFactory(src);

                //if (nodes.TryGetValue(src, out var node))
                //{
                //    graphEdges.Add(new GraphEdge<TNode, TEdge>(

                //        edge,
                //        node,

                //    ));
                //    edge.Source = node;
                //}

                //edge.Source.OutgoingEdges.Add(edge);
                //edge.Destination.IncomingEdges.Add(edge);
            }

            return new Graph<TNode, TEdge>
            {
                Edges = graphEdges,
                //Nodes = nodes,
                //Sources = nodes.Where(n => n.IncomingEdges.Count == 0),
                //Sinks = nodes.Where(n => n.OutgoingEdges.Count == 0)
            };
        }
    }

    public sealed class GraphNode<TNode, TEdge>
    {
        public GraphNode(TNode value)
        {
        }

        public Graph<TNode, TEdge> ParentGraph { get; }

        public ICollection<GraphEdge<TNode, TEdge>> IncomingEdges { get; }

        public ICollection<GraphEdge<TNode, TEdge>> OutgoingEdges { get; }

        public TNode Value { get; set; }
    }

    public sealed class GraphEdge<TNode, TEdge>
    {
        public GraphEdge(TEdge value, GraphNode<TNode, TEdge> source, GraphNode<TNode, TEdge> destination, Graph<TNode, TEdge> parentGraph)
        {
            Value = value;
            Source = source;
            Destination = destination;
            ParentGraph = parentGraph;
        }

        public Graph<TNode, TEdge> ParentGraph { get; }

        public GraphNode<TNode, TEdge> Source { get; }

        public GraphNode<TNode, TEdge> Destination { get; }

        public TEdge Value { get; set; }
    }
}
