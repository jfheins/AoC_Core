using System;
using System.Collections.Generic;
using System.Linq;
using C5;
using Core;

namespace Core
{
    public class Cube : IComparable<Cube>
    {
        public Point3 BottomLeft { get; set; }

        /// <summary>
        /// Point that is NOT in the cube any more
        /// </summary>
        public Point3 TopRight => BottomLeft.TranslateBy(SideLength, SideLength, SideLength);
        public int SideLength { get; set; }
        public IPriorityQueueHandle<Cube> Handle { get; set; } = null;

        public int NanobotsInRange { get; set; } = 0;

        public Cube(Point3 root, int side)
        {
            BottomLeft = root;
            SideLength = side;
        }

        public int CompareTo(Cube other)
        {
            return NanobotsInRange.CompareTo(other.NanobotsInRange);
        }

        public Point3 Center => BottomLeft.TranslateBy(SideLength / 2, SideLength / 2, SideLength / 2);

        public IEnumerable<ILineSegment> GetEdges()
        {
            for (int x = BottomLeft.X; x < TopRight.X; x++)
            {
                yield return new Point3(x, BottomLeft.Y, BottomLeft.Z);
                yield return new Point3(x, BottomLeft.Y, TopRight.Z - 1);
                yield return new Point3(x, TopRight.Y - 1, BottomLeft.Z);
                yield return new Point3(x, TopRight.Y - 1, TopRight.Z - 1);
            }
            for (int y = BottomLeft.Y; y < TopRight.Y; y++)
            {
                yield return new Point3(BottomLeft.X, y, BottomLeft.Z);
                yield return new Point3(BottomLeft.X, y, TopRight.Z - 1);
                yield return new Point3(TopRight.X - 1, y, BottomLeft.Z);
                yield return new Point3(TopRight.X - 1, y, TopRight.Z - 1);
            }
            for (int z = BottomLeft.Z; z < TopRight.Z; z++)
            {
                yield return new Point3(BottomLeft.X, BottomLeft.Y, z);
                yield return new Point3(BottomLeft.X, TopRight.Y - 1, z);
                yield return new Point3(TopRight.X - 1, BottomLeft.Y, z);
                yield return new Point3(TopRight.X - 1, TopRight.Y - 1, z);
            }
        }

        public IEnumerable<Point3> GetEdgePoints()
        {
            for (int x = BottomLeft.X; x < TopRight.X; x++)
            {
                yield return new Point3(x, BottomLeft.Y, BottomLeft.Z);
                yield return new Point3(x, BottomLeft.Y, TopRight.Z - 1);
                yield return new Point3(x, TopRight.Y - 1, BottomLeft.Z);
                yield return new Point3(x, TopRight.Y - 1, TopRight.Z - 1);
            }
            for (int y = BottomLeft.Y; y < TopRight.Y; y++)
            {
                yield return new Point3(BottomLeft.X, y, BottomLeft.Z);
                yield return new Point3(BottomLeft.X, y, TopRight.Z - 1);
                yield return new Point3(TopRight.X - 1, y, BottomLeft.Z);
                yield return new Point3(TopRight.X - 1, y, TopRight.Z - 1);
            }
            for (int z = BottomLeft.Z; z < TopRight.Z; z++)
            {
                yield return new Point3(BottomLeft.X, BottomLeft.Y, z);
                yield return new Point3(BottomLeft.X, TopRight.Y - 1, z);
                yield return new Point3(TopRight.X - 1, BottomLeft.Y, z);
                yield return new Point3(TopRight.X - 1, TopRight.Y - 1, z);
            }
        }

        public static IEnumerable<Cube> Split(Cube c)
        {
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

    internal struct LineSegmentX
    {
        int MinX;
        int MaxX;
        int Y;
        int Z;

        public Point3 ClosestPointTo(Point3 p)
        {
            if (p.X < MinX)
            {
                return
            }
        }
    }
}
