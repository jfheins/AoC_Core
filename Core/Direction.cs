using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public enum Direction8
    {
        UpLeft,
        Up,
        UpRight,
        Left,
        Right,
        DownLeft,
        Down,
        DownRight
    }

    public static class Directions
    {
        public static Direction[] Vertical => new Direction[] { Direction.Up, Direction.Down };
        public static Direction[] Horizontal => new Direction[] { Direction.Left, Direction.Right };

        public static Direction[] AntiReading => new Direction[] { Direction.Left, Direction.Up };
        public static Direction[] Reading => new Direction[] { Direction.Right, Direction.Down };
        public static Direction[] All4 => new Direction[] { Direction.Left, Direction.Up, Direction.Right, Direction.Down };
        public static Direction8[] All8 => new Direction8[] { Direction8.UpLeft, Direction8.Up, Direction8.UpRight,
            Direction8.Left, Direction8.Right, Direction8.DownLeft, Direction8.Down, Direction8.DownRight };
    }

    public static class DirectionExtensions
    {
        private static readonly Dictionary<Direction, Size> _mapDirectionToSize = new()
        {
            { Direction.Left, new Size(-1, 0) },
            { Direction.Up, new Size(0, -1) },
            { Direction.Right, new Size(1, 0) },
            { Direction.Down, new Size(0, 1) }
        };

        public static Size ToSize(this Direction dir) => _mapDirectionToSize[dir];
        public static Point MoveTo(this Point p, Direction dir, int steps = 1) => p + (steps * _mapDirectionToSize[dir]);
        public static IEnumerable<Point> MoveLURD(this Point p)
            => ((IEnumerable<Size>)_mapDirectionToSize.Values).Select(s => p + s);
        public static IEnumerable<Point> MoveLURDDiag(this Point p)
        {
            var sizes = new[] {  new Size(-1, -1), new Size(0, -1),  new Size(1, -1),
                new Size(-1, 0), new Size(1, 0),
                new Size(-1, 1), new Size(0, 1),  new Size(1, 1),
            };
            return sizes.Select(s => p + s);
        }
        public static int Manhattan(this Point p) => Math.Abs(p.X) + Math.Abs(p.Y);
        public static Point TurnClockwise(this Point p, int degrees)
        {
            var rad = degrees * Math.PI / 180;
            var x = p.X * Math.Cos(rad) - p.Y * Math.Sin(rad);
            var y = p.X * Math.Sin(rad) + p.Y * Math.Cos(rad);
            return new Point(Convert.ToInt32(x), Convert.ToInt32(y));
        }
        public static Point TurnCounterClockwise(this Point p, int degrees) => p.TurnClockwise(-degrees);

        public static Direction TurnClockwise(this Direction dir, int times = 1) => (Direction)(((int)dir + times) % 4);
        public static Direction TurnCounterClockwise(this Direction dir, int times = 1) => (Direction)(((int)dir + (3 * times)) % 4);
        public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);


        public static Direction ToDirection(this ConsoleKeyInfo key) => (Direction)(key.Key - 37);
    }
}
