using System;
using System.Numerics;

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

        public bool IsEmpty => this.Equals(Empty);
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool Equals(Point3 other) => (X == other.X) && (Y == other.Y) && (Z == other.Z);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override bool Equals(object? obj) => (obj is Point3 p) && Equals(p);

        public Point3 TranslateBy(int dx, int dy, int dz) => new Point3(X + dx, Y + dy, Z + dz);
        public Point3 TranslateBy((int dx, int dy, int dz) velocity)
            => new Point3(X + velocity.dx, Y + velocity.dy, Z + velocity.dz);
        public Point3 TranslateBy(Vector3 velocity)
            => new Point3(X + (int)velocity.X, Y + (int)velocity.Y, Z + (int)velocity.Z);

        public static Point3 FromArray(int[] arr, int offset = 0)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));

            if (arr.Length < offset + 3)
                throw new ArgumentException("Not enough elements in the array!");

            return new Point3(arr[offset], arr[offset + 1], arr[offset + 2]);
        }

        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3 left, Point3 right)
        {
            return !(left == right);
        }

        public override string ToString() => $"<X: {X}, Y: {Y}, Z: {Z}>";
    }
}
