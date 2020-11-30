using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public class Grid2<TNode> where TNode : notnull
    {
        public int? MinX { get; private set; } = null;
        public int? MaxX { get; private set; } = null;
        public int? MinY { get; private set; } = null;
        public int? MaxY { get; private set; } = null;

        public Point Origin { get; set; } = Point.Empty;

        private readonly Func<Point, TNode> _nodeDataCallback;
        private readonly Dictionary<Point, TNode> _nodeDataCache = new();

        public Grid2(Func<Point, TNode> dataCallback)
        {
            _nodeDataCallback = dataCallback;
        }

        public TNode this[int x, int y]
        {
            get => this[new Point(x, y)];
            set => this[new Point(x, y)] = value;
        }

        public TNode this[Point p]
        {
            get => _nodeDataCache.GetOrAdd(p, _nodeDataCallback);
            set => _nodeDataCache[p] = value;
        }
    }
}
