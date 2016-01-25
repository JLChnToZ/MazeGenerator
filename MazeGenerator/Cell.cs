using System;

namespace JLChnToZ.MazeGenerator {
    public class Cell {
        readonly int[] coords;
        int flag;

        public Cell(int dimensions, params int[] coordinates) {
            coords = new int[dimensions];
            int length = coordinates.Length;
            if (length > 0)
                Array.Copy(coordinates, coords, Math.Min(dimensions, length));
        }

        public Cell(int dimensions, bool isWallPreBuilt, params int[] coordinates) : this(dimensions, coordinates) {
            if(!isWallPreBuilt)
                flag = ~0;
        }

        public bool HasWall(int axisIndex, bool backward) {
            if (axisIndex < 0) throw new ArgumentOutOfRangeException("axisIndex");
            return !flag.HasBit(axisIndex * 2 + (backward ? 1 : 0));
        }

        public void Connect(int axisIndex, bool backward) {
            if (axisIndex < 0) throw new ArgumentOutOfRangeException("axisIndex");
            flag = flag.SetBit(axisIndex * 2 + (backward ? 1 : 0), true);
        }

        public void Disconnect(int axisIndex, bool backward) {
            if (axisIndex < 0) throw new ArgumentOutOfRangeException("axisIndex");
            flag = flag.SetBit(axisIndex * 2 + (backward ? 1 : 0), false);
        }

        public int[] Coordinates {
            get { return coords.Clone() as int[]; }
        }

        public int Flag {
            get { return flag; }
            set { flag = value; }
        }
    }
}
