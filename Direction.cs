using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }

    public static class DirectionExtensions
    {
        private static readonly Dictionary<Direction, Size> _mapDirectionToSize = new Dictionary<Direction, Size>
            {
                {Direction.Left, new Size(-1, 0)},
                {Direction.Up, new Size(0, -1)},
                {Direction.Right, new Size(1, 0)},
                {Direction.Down, new Size(0, 1)}
            };

        public static Size ToSize(this Direction dir) => _mapDirectionToSize[dir];
        public static Point MoveTo(this Point p, Direction dir) => p + _mapDirectionToSize[dir];

        public static Direction TurnClockwise(this Direction dir) => (Direction)(((int)dir + 1) % 4);
        public static Direction TurnCounterClockwise(this Direction dir) => (Direction)(((int)dir + 3) % 4);
        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);

    }
}
