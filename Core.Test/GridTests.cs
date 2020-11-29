using Microsoft.VisualStudio.TestTools.UnitTesting;

using Core;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Core.Test
{
    [TestClass]
    public class GridTests
    {
        [TestMethod]
        public void OriginZeroByDefault()
        {
            var grid = new Grid2<int>(x => 0);

            Assert.AreEqual(grid.Origin, Point.Empty);
        }

        [TestMethod]
        public void GridGetsPopulated()
        {
            var grid = new Grid2<int>(p => p.X + p.Y);

            Assert.AreEqual(grid[1, 1], 2);
        }
    }
}
