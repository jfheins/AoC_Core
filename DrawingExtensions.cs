using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Core
{
    public static class DrawingExtensions
    {
        public static Rectangle Clone(this Rectangle rect) => new Rectangle(rect.Location, rect.Size);

        public static Rectangle InflatedCopy(this Rectangle rect, int width, int height)
        {
            var newRect = new Rectangle(rect.Location, rect.Size);
            newRect.Inflate(width, height);
            return newRect;
        }
    }
}
