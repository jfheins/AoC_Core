using System;

namespace Core
{
    public record Cuboid
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

        public long Size => checked (Width * (long)Height * Depth);

        public Cuboid? Intersect(Cuboid other)
        {
            var tr = TopRight;
            var otr = other.TopRight;
            var x = IntersectLine(Location.X, tr.X, other.Location.X, otr.X);
            var y = IntersectLine(Location.Y, tr.Y, other.Location.Y, otr.Y);
            var z = IntersectLine(Location.Z, tr.Z, other.Location.Z, otr.Z);

            if (!x.isValid || !y.isValid || !z.isValid)
                return null;
            return new Cuboid
            {
                Location = new Point3(x.start, y.start, z.start),
                Width = x.end - x.start,
                Height = y.end - y.start,
                Depth = z.end - z.start,
            };

            static (int start, int end, bool isValid) IntersectLine(int start1, int exend1, int start2, int exend2)
            {
                // End is excluusive!
                var start = Math.Max(start1, start2);
                var end = Math.Min(exend1, exend2);
                return (start, end, start < end);
            }
        }
    }
}
