using System;
using System.Collections.Generic;
using JLChnToZ.MultiDimensionalArray;

namespace JLChnToZ.MazeGenerator {
    public interface IMazeGenerator: ISizeObject, ICollection<Cell> {
        void SetSize(params int[] axisLength);
        void SetSizeAxis(int axis, int length);
        void GenerateMaze(int[] startCoord, int[] endCoord);
        Array GetMazeAligned();
        Cell GetCell(params int[] coordinate);
    }
}
