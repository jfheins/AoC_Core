using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Core
{
    public interface ICuboid
    {
        Point3 Location { get; }
        int Width { get; }
        int Height { get; }
        int Depth { get; }
        Point3 TopRight { get; }
    }

    public record Cuboid : ICuboid
    {
        public Point3 Location { get; init; }
        /// <summary>
        /// Size in X
        /// </summary>
        public int Width { get; init; }
        /// <summary>
        /// Size in Y
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Size in Z
        /// </summary>
        public int Depth { get; init; }

        public Point3 TopRight => Location.TranslateBy(Width, Height, Depth);

        public long Size => checked(Width * (long)Height * Depth);

        public Cuboid? Intersect(ICuboid other)
        {
            var start = Sse41.Max(Location.AsVector128(), other.Location.AsVector128());
            var end = Sse41.Min(TopRight.AsVector128(), other.TopRight.AsVector128());

            var isvalid = Sse2.CompareLessThan(start, end).Equals(Vector128.Create(-1, -1, -1, 0));
            if (isvalid)
            {
                var diff = Sse2.Subtract(end, start);
                return new Cuboid
                {
                    Location = Point3.FromVector128(start),
                    Width = diff.GetElement(0),
                    Height = diff.GetElement(1),
                    Depth = diff.GetElement(2),
                };
            }
            else
                return null;
        }
    }
}
