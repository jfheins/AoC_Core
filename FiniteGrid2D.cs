using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Core
{
    public class FiniteGrid2D<TNode> : ICollection<(Point pos, TNode value)>
        where TNode : notnull
    {
        public Rectangle Bounds { get; private set; }
        public int Count { get; }
        public bool IsReadOnly { get; }

        private readonly Dictionary<Point, TNode> _values = new Dictionary<Point, TNode>();


        public FiniteGrid2D(int width, int height, Func<int, int, TNode> dataCallback)
            : this(width, height, p => dataCallback(p.X, p.Y)) { }

        public FiniteGrid2D(Size size, Func<Point, TNode> dataCallback)
            : this(size.Width, size.Height, dataCallback) { }

        public FiniteGrid2D(int width, int height, Func<Point, TNode> dataCallback)
        {
            Bounds = new Rectangle(0, 0, width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = new Point(x, y);
                    _values[p] = dataCallback(p);
                }
            }
        }
        public FiniteGrid2D(IEnumerable<IEnumerable<TNode>> data)
        {
            foreach (var (x, y, value) in data.WithXY())
                this[x, y] = value;
            var maxx = _values.Max(kvp => kvp.Key.X);
            var maxy = _values.Max(kvp => kvp.Key.Y);
            Bounds = new Rectangle(0, 0, maxx + 1, maxy + 1);
        }
        public FiniteGrid2D(FiniteGrid2D<TNode> source)
        {
            _values = new Dictionary<Point, TNode>(source._values);
            Bounds = source.Bounds;
        }

        public TNode this[int x, int y]
        {
            get => this[new Point(x, y)];
            set => this[new Point(x, y)] = value;
        }

        public TNode this[Point pos]
        {
            get => _values[pos];
            set => _values[pos] = value;
        }

        public TNode GetValueOrDefault(Point pos, TNode defaultValue = default)
            => _values.GetValueOrDefault(pos, defaultValue);

        public IEnumerable<Point> Get4NeighborsOf(Point pos)
            => pos.MoveLURD().Where(Bounds.Contains);

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    sb.Append(this[x, y].ToString());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Add((Point pos, TNode value) item) => _values.Add(item.pos, item.value);
        public void Clear() => _values.Clear();
        public bool Contains((Point pos, TNode value) item)
            => _values.Contains(new KeyValuePair<Point, TNode>(item.pos, item.value));
        public void CopyTo((Point pos, TNode value)[] array, int arrayIndex)
            => throw new NotImplementedException();
        public bool Remove((Point pos, TNode value) item) => _values.Remove(item.pos);
        public IEnumerator<(Point pos, TNode value)> GetEnumerator() => new EnumWrapper(_values);
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();


        private struct EnumWrapper : IEnumerator<(Point pos, TNode value)>
        {
            private readonly IEnumerator<KeyValuePair<Point, TNode>> _enumerator;

            public EnumWrapper(Dictionary<Point, TNode> values) : this()
            {
                this._enumerator = values.GetEnumerator();
            }

            private (Point pos, TNode value) Convert(KeyValuePair<Point, TNode> kvp)
                => (kvp.Key, kvp.Value);

            public (Point pos, TNode value) Current => Convert(_enumerator.Current);
            object? IEnumerator.Current => Current;

            public void Dispose() => _enumerator.Dispose();
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => _enumerator.Reset();
        }
    }

}
