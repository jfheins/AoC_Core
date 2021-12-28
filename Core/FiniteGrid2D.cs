using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Core
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collection is a minor concern")]
    public class FiniteGrid2D<TNode> : ICollection<(Point pos, TNode value)>
        where TNode : notnull
    {
        public Rectangle Bounds { get => _bounds.Clone(); private set => _bounds = value; }
        public int Count => _values.Count;
        public bool IsReadOnly { get; }
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Size Size => Bounds.Size;
        public Point TopLeft => Bounds.Location;
        public Point BottomRight => new(Width - 1, Height - 1);

        protected readonly Dictionary<Point, TNode> _values = new();
        private Rectangle _bounds;

        public FiniteGrid2D(int width, int height, TNode value)
            : this(width, height, p => value) { }

        public FiniteGrid2D(int width, int height, Func<int, int, TNode> dataCallback)
            : this(width, height, p => dataCallback(p.X, p.Y)) { }

        public FiniteGrid2D(Size size, Func<Point, TNode> dataCallback)
            : this(new Rectangle(Point.Empty, size), dataCallback) { }

        public FiniteGrid2D(int width, int height, Func<Point, TNode> dataCallback)
            : this(new Rectangle(0, 0, width, height), dataCallback) { }

        public FiniteGrid2D(Rectangle bounds, Func<Point, TNode> dataCallback)
        {
            Bounds = bounds;
            Fill(dataCallback);
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
            Contract.Assert(source != null);
            _values = new Dictionary<Point, TNode>(source._values);
            Bounds = source.Bounds;
        }
        public FiniteGrid2D(FiniteGrid2D<TNode> source, int inflation, TNode fillValue)
        {
            Contract.Assert(source != null);
            Contract.Assert(inflation >= 0, "inflation must be positive");

            Bounds = source.Bounds.InflatedCopy(inflation, inflation);
            Fill(p => source.GetValueOrDefault(p, fillValue));
        }

        private void Fill(Func<Point, TNode> dataCallback)
        {
            for (int y = Bounds.Left; y < Bounds.Height; y++)
            {
                for (int x = Bounds.Top; x < Bounds.Width; x++)
                {
                    var p = new Point(x, y);
                    var val = dataCallback(p);
                    if (val != null)
                        _values[p] = dataCallback(p);
                }
            }
        }


        public virtual bool Contains(Point pos) => Bounds.Contains(pos);

        public virtual TNode this[int x, int y]
        {
            get => this[new Point(x, y)];
            set => this[new Point(x, y)] = value;
        }

        public virtual TNode this[Point pos]
        {
            get => _values[pos];
            set => _values[pos] = value;
        }

        public virtual TNode GetValueOrDefault(Point pos, TNode defaultValue)
            => _values.GetValueOrDefault(pos, defaultValue);

        private static int Wrap(int coord, int limit) => ((coord % limit) + limit) % limit;
        public virtual TNode GetValueWraparound(Point p) => GetValueWraparound(p.X, p.Y);
        public virtual TNode GetValueWraparound(int x, int y)
        {
            var wrapped = new Point(Wrap(x, Width), Wrap(y, Height));
            return _values[wrapped];

        }
        public virtual (Point pos, TNode value) GetTupleWraparound(Point p) => GetTupleWraparound(p.X, p.Y);
        public virtual (Point pos, TNode value) GetTupleWraparound(int x, int y)
        {
            var wrapped = new Point(Wrap(x, Width), Wrap(y, Height));
            return (wrapped, _values[wrapped]);
        }

        public virtual void SetValueWraparound(Point p, TNode value)
        {
            var wrapped = new Point(Wrap(p.X, Width), Wrap(p.Y, Height));
            _values[wrapped] = value;

            static int Wrap(int coord, int limit) => ((coord % limit) + limit) % limit;
        }

        public virtual IEnumerable<Point> Get4NeighborsOf(Point pos)
            => pos.MoveLURD().Where(Contains);

        public virtual IEnumerable<Point> Get8NeighborsOf(Point pos)
            => pos.MoveLURDDiag().Where(Contains);

        public IEnumerable<TNode> GetPointWith8Neighbors(Point pos, TNode defaultValue)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    yield return _values.GetValueOrDefault(pos.MoveBy(dx, dy), defaultValue);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int y = Bounds.Top; y < Bounds.Height; y++)
            {
                for (int x = Bounds.Left; x < Bounds.Width; x++)
                {
                    if (_values.TryGetValue(new Point(x, y), out var v))
                        sb.Append(v.ToString());
                    else
                        sb.Append('?');
                }
                _ = sb.AppendLine();
            }
            return sb.ToString();
        }

        public IEnumerable<Point> Line(Point exclusiveStart, Size direction)
        {
            return Enumerable.Range(1, int.MaxValue).Select(n => exclusiveStart + n * direction)
                .TakeWhile(Contains);
        }

        public void Add((Point pos, TNode value) item) => _values.Add(item.pos, item.value);
        public void Clear() => _values.Clear();
        public bool Contains((Point pos, TNode value) item)
            => _values.Contains(new KeyValuePair<Point, TNode>(item.pos, item.value));
        public void CopyTo((Point pos, TNode value)[] array, int arrayIndex)
        {
            foreach (var kvp in _values)
                array[arrayIndex++] = (kvp.Key, kvp.Value);
        }

        public bool Remove((Point pos, TNode value) item) => _values.Remove(item.pos);
        public IEnumerator<(Point pos, TNode value)> GetEnumerator() => new EnumWrapper(_values);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<TNode> GetEdge(Direction dir)
        {
            var dx = Enumerable.Empty<int>();
            var dy = Enumerable.Empty<int>();
            if (dir == Direction.Up)
            {
                dx = Enumerable.Range(Bounds.X, Bounds.Width);
                dy = Bounds.Y.ToEnumerable();
            }
            if (dir == Direction.Down)
            {
                dx = Enumerable.Range(Bounds.X, Bounds.Width);
                dy = (Bounds.Bottom - 1).ToEnumerable();
            }
            if (dir == Direction.Left)
            {
                dx = Bounds.X.ToEnumerable();
                dy = Enumerable.Range(Bounds.Y, Bounds.Height);
            }
            if (dir == Direction.Right)
            {
                dx = (Bounds.Right - 1).ToEnumerable();
                dy = Enumerable.Range(Bounds.Y, Bounds.Height);
            }
            return dx.CartesianProduct(dy).Select(t => this[t.Item1, t.Item2]);
        }


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
