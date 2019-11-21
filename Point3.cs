using System;

namespace Core
{
    public struct Point3 : IEquatable<Point3>
    {
        public static readonly Point3 Empty = new Point3(0, 0, 0);

        public Point3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool IsEmpty => this.Equals(Point3.Empty);
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool Equals(Point3 other)
        {
            return (X == other.X) && (Y == other.Y) && (Z == other.Z);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override bool Equals(object obj)
        {
            return (obj is Point3 p) && Equals(p);
        }

        public Point3 TranslateBy(int dx, int dy, int dz)
        {
            return new Point3(X + dx, Y + dy, Z + dz);
        }
    }
}
