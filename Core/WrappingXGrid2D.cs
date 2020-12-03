using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Core
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collection is a minor concern")]
    public class WrappingXGrid2D<TNode> :FiniteGrid2D<TNode>
        where TNode : notnull
    {
        public int Width { get; private set; } = int.MaxValue;

        public WrappingXGrid2D(int width, int height, TNode value) : base(width, height, value)
        {
            Width = Bounds.Width;
        }

        public WrappingXGrid2D(int width, int height, Func<int, int, TNode> dataCallback) : base(width, height, dataCallback)
        {
            Width = Bounds.Width;
        }

        public WrappingXGrid2D(Size size, Func<Point, TNode> dataCallback) : base(size, dataCallback)
        {
            Width = Bounds.Width;
        }

        public WrappingXGrid2D(int width, int height, Func<Point, TNode> dataCallback) : base(width, height, dataCallback)
        {
            Width = Bounds.Width;
        }

        public WrappingXGrid2D(IEnumerable<IEnumerable<TNode>> data) : base(data)
        {
            Width = Bounds.Width;
        }

        public WrappingXGrid2D(FiniteGrid2D<TNode> source) : base(source)
        {
            Width = Bounds.Width;
        }

        private Point Wrap(Point p) => new(p.X % Width, p.Y);

        public override bool Contains(Point pos) => pos.Y >= Bounds.Y && pos.Y < Bounds.Height;

        public override TNode this[Point pos]
        {
            get => _values[Wrap(pos)];
            set => _values[Wrap(pos)] = value;
        }

        public override TNode GetValueOrDefault(Point pos, TNode defaultValue)
            => _values.GetValueOrDefault(Wrap(pos), defaultValue);
    }

}
