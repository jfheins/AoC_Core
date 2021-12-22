using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Intrinsics;

namespace Core
{
    public struct Point3 : IEquatable<Point3>, ICanEnumerateNeighbors<Point3>
    {
        public static readonly Point3 Empty = new(0, 0, 0);

        public Point3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public override string ToString() => $"<X: {X}, Y: {Y}, Z: {Z}>";

        public bool IsEmpty => this.Equals(Empty);
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool Equals(Point3 other) => (X == other.X) && (Y == other.Y) && (Z == other.Z);

        public override int GetHashCode() => HashCode.Combine(X, Y, Z);

        public override bool Equals(object? obj) => (obj is Point3 p) && Equals(p);

        public Point3 TranslateBy(int dx, int dy, int dz) => new(X + dx, Y + dy, Z + dz);
        public Point3 TranslateBy((int dx, int dy, int dz) velocity)
            => new(X + velocity.dx, Y + velocity.dy, Z + velocity.dz);
        public Point3 TranslateBy(Vector3 velocity)
            => new(X + (int)velocity.X, Y + (int)velocity.Y, Z + (int)velocity.Z);

        public Point3 TranslateBy(Point3 offset)
            => TranslateBy(offset.X, offset.Y, offset.Z);

        public static Point3 FromArray(int[] arr, int offset = 0)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));

            if (arr.Length < offset + 3)
                throw new ArgumentException("Not enough elements in the array!");

            return new Point3(arr[offset], arr[offset + 1], arr[offset + 2]);
        }


        public IEnumerable<Point3> GetNeighborsDiag()
        {
            var deltas = new[] { -1, 0, 1 };
            foreach (var dx in deltas)
                foreach (var dy in deltas)
                    foreach (var dz in deltas)
                        if (dx != 0 || dy != 0 || dz != 0)
                            yield return TranslateBy(dx, dy, dz);
        }

        public static bool operator ==(Point3 left, Point3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3 left, Point3 right)
        {
            return !(left == right);
        }

        public static Point3 operator +(Point3 l, Point3 r)
            => new(l.X + r.X, l.Y + r.Y, l.Z + r.Z);

        public static Point3 operator -(Point3 l, Point3 r)
            => new (l.X - r.X, l.Y - r.Y, l.Z - r.Z);

        public Point3 Inverse() => new(-X, -Y, -Z);

        public Vector3 ToVector() => new(X, Y, Z);

        public Vector128<int> AsVector128() => Vector128.Create(X, Y, Z, 0);

        public static Point3 FromVector128(Vector128<int> v) => new(v.GetElement(0), v.GetElement(1), v.GetElement(2));
    }
}
