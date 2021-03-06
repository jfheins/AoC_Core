﻿using System;
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
        protected Rectangle Bounds { get; private set; }
        public int Count => _values.Count;
        public bool IsReadOnly { get; }
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;

        protected readonly Dictionary<Point, TNode> _values = new();


        public FiniteGrid2D(int width, int height, TNode value)
            : this(width, height, p => value) { }

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
            Contract.Assert(source != null);
            _values = new Dictionary<Point, TNode>(source._values);
            Bounds = source.Bounds;
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

        public virtual IEnumerable<Point> Get4NeighborsOf(Point pos)
            => pos.MoveLURD().Where(Contains);

        public virtual IEnumerable<Point> Get8NeighborsOf(Point pos)
    => pos.MoveLURDDiag().Where(Contains);

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Bounds.Height; y++)
            {
                for (int x = 0; x < Bounds.Width; x++)
                {
                    _ = sb.Append(this[x, y].ToString());
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
            => throw new NotImplementedException();
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
