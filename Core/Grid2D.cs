using System;
using System.IO;

namespace Core
{
    public static class Grid2D
    {

        public static FiniteGrid2D<char> FromFile(string filePath)
        {
            var content = File.ReadAllLines(filePath);
           return new FiniteGrid2D<char>(content);
        }
        public static FiniteGrid2D<char> FromFile(string filePath, Range lines, Range columns)
        {
            var content = File.ReadAllLines(filePath)[lines];
            for (int i = 0; i < content.Length; i++)
                content[i] = content[i][columns];

            return new FiniteGrid2D<char>(content);
        }
    }
}
