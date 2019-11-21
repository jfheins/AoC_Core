using System;

namespace Core
{
    public struct Point4 : IEquatable<Point4>
    {
        public static readonly Point4 Empty = new Point4(0, 0, 0, 0);

        public Point4(int x, int y, int z, int t)
        {
            X = x;
            Y = y;
            Z = z;
            T = t;
        }

        public bool IsEmpty => this.Equals(Empty);
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int T { get; set; }


        public override bool Equals(object obj) => (obj is Point4 p) && this.Equals(p);
        public bool Equals(Point4 other) => (X == other.X) && (Y == other.Y) && (Z == other.Z) && (T == other.T);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z, T);

        public Point4 TranslateBy(int dx, int dy, int dz, int dt) => new Point4(X + dx, Y + dy, Z + dz, T + dt);
    }
}
