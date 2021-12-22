using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using Core;

namespace Core
{
    public class Cube : ICuboid
    {
        public Point3 BottomLeft { get; set; }

        /// <summary>
        /// Point that is NOT in the cube any more
        /// </summary>
        public Point3 TopRight => BottomLeft.TranslateBy(SideLength, SideLength, SideLength);
        public int SideLength { get; set; }

        public Cube(Point3 root, int side)
        {
            BottomLeft = root;
            SideLength = side;
        }

        public Point3 Center => BottomLeft.TranslateBy(SideLength / 2, SideLength / 2, SideLength / 2);

        Point3 ICuboid.Location => BottomLeft;

        int ICuboid.Width => SideLength;

        int ICuboid.Height => SideLength;

        int ICuboid.Depth => SideLength;

        public IEnumerable<ILineSegment> GetEdges()
        {
            // Inclusive bound that is still part of the cube
            var inclBlound = TopRight.TranslateBy(-1, -1, -1);

            yield return new LineSegmentX(BottomLeft.X, inclBlound.X, BottomLeft.Y, BottomLeft.Z);
            yield return new LineSegmentX(BottomLeft.X, inclBlound.X, BottomLeft.Y, inclBlound.Z);
            yield return new LineSegmentX(BottomLeft.X, inclBlound.X, inclBlound.Y, BottomLeft.Z);
            yield return new LineSegmentX(BottomLeft.X, inclBlound.X, inclBlound.Y, inclBlound.Z);

            yield return new LineSegmentY(BottomLeft.X, BottomLeft.Y, inclBlound.Y, BottomLeft.Z);
            yield return new LineSegmentY(BottomLeft.X, BottomLeft.Y, inclBlound.Y, inclBlound.Z);
            yield return new LineSegmentY(inclBlound.X, BottomLeft.Y, inclBlound.Y, BottomLeft.Z);
            yield return new LineSegmentY(inclBlound.X, BottomLeft.Y, inclBlound.Y, inclBlound.Z);

            yield return new LineSegmentZ(BottomLeft.X, BottomLeft.Y, BottomLeft.Z, inclBlound.Z);
            yield return new LineSegmentZ(BottomLeft.X, inclBlound.Y, BottomLeft.Z, inclBlound.Z);
            yield return new LineSegmentZ(inclBlound.X, BottomLeft.Y, BottomLeft.Z, inclBlound.Z);
            yield return new LineSegmentZ(inclBlound.X, inclBlound.Y, BottomLeft.Z, inclBlound.Z);
        }

        public static IEnumerable<Cube> Split(Cube c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            var halfSide = c.SideLength / 2;
            yield return new Cube(c.BottomLeft, halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(halfSide, 0, 0), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(0, halfSide, 0), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(0, 0, halfSide), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(halfSide, halfSide, 0), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(halfSide, 0, halfSide), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(0, halfSide, halfSide), halfSide);
            yield return new Cube(c.BottomLeft.TranslateBy(halfSide, halfSide, halfSide), halfSide);
        }

        public bool Contains(Point3 p)
        {
            return (p.X >= BottomLeft.X && p.X < TopRight.X)
                && (p.Y >= BottomLeft.Y && p.Y < TopRight.Y)
                && (p.Z >= BottomLeft.Z && p.Z < TopRight.Z);
        }
    }

    public interface ILineSegment
    {
        public Point3 ClosestPointTo(Point3 p);
    }

    readonly struct LineSegmentX : ILineSegment
    {
        public readonly int MinX;
        public readonly int MaxX;
        public readonly int Y;
        public readonly int Z;

        public LineSegmentX(int x1, int x2, int y, int z)
        {
            MinX = x1;
            MaxX = x2;
            Y = y;
            Z = z;
        }

        public Point3 ClosestPointTo(Point3 p)
        {
            if (p.X <= MinX)
                return new Point3(MinX, Y, Z);
            if (p.X >= MaxX)
                return new Point3(MaxX, Y, Z);
            return new Point3(p.X, Y, Z);
        }
    }

    readonly struct LineSegmentY : ILineSegment
    {
        public readonly int X;
        public readonly int MinY;
        public readonly int MaxY;
        public readonly int Z;

        public LineSegmentY(int x, int y1, int y2, int z)
        {
            X = x;
            MinY = y1;
            MaxY = y2;
            Z = z;
        }
        public Point3 ClosestPointTo(Point3 p)
        {
            if (p.Y <= MinY)
                return new Point3(X, MinY, Z);
            if (p.Y >= MaxY)
                return new Point3(X, MaxY, Z);
            return new Point3(X, p.Y, Z);
        }
    }

    readonly struct LineSegmentZ : ILineSegment
    {
        public readonly int X;
        public readonly int Y;
        public readonly int MinZ;
        public readonly int MaxZ;

        public LineSegmentZ(int x, int y, int z1, int z2)
        {
            X = x;
            Y = y;
            MinZ = z1;
            MaxZ = z2;
        }

        public Point3 ClosestPointTo(Point3 p)
        {
            if (p.Z <= MinZ)
                return new Point3(X, Y, MinZ);
            if (p.Z >= MaxZ)
                return new Point3(X, Y, MaxZ);
            return new Point3(X, Y, p.Z);
        }
    }
}
