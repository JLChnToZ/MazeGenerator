using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JLChnToZ.MultiDimensionalArray;

namespace JLChnToZ.MazeGenerator {
    public class DefaultMazeGenerator : IMazeGenerator {
        readonly int dimensions;
        readonly int[] sizes;
        readonly Array<Cell> cells;
        Random random;
        object locker;

        public DefaultMazeGenerator(int dimensions) {
            if (dimensions < 2)
                throw new ArgumentOutOfRangeException("dimensions");
            this.dimensions = dimensions;
            this.sizes = new int[dimensions];
            this.cells = new Array<Cell>(new int[dimensions]);
            this.locker = new object();
        }

        public int Count {
            get { return cells.Count; }
        }

        bool ICollection<Cell>.IsReadOnly {
            get { return true; }
        }

        public Random RandomGenerator {
            get { return random; }
            set { lock (locker) random = value; }
        }

        void ICollection<Cell>.Add(Cell item) { throw new NotSupportedException(); }

        bool ICollection<Cell>.Remove(Cell item) { throw new NotSupportedException(); }

        bool ICollection<Cell>.Contains(Cell item) {
            return cells.Contains(item);
        }

        public void Clear() {
            cells.Clear();
        }

        public void CopyTo(Cell[] array, int arrayIndex) {
            cells.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return cells.GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator() {
            return cells.GetEnumerator();
        }

        public Array GetMazeAligned() {
#if !ENABLE_IL2CPP
            var result = Array.CreateInstance(typeof(Cell), sizes);
            foreach (var kv in cells as IDictionary<CoordIndexer, Cell>)
                result.SetValue(kv.Value, kv.Key.Coordinates);
            return result;
#else
            throw new NotSupportedException("IL2CPP does not support multi-dimensional array.");
#endif
        }

        public int Dimensions {
            get { return dimensions; }
        }

        public int GetSize(int axis) {
            if (axis < 0 || axis >= dimensions)
                throw new ArgumentOutOfRangeException("axis");
            return sizes[axis];
        }

        public void SetSize(params int[] axisLength) {
            lock (locker) {
                foreach (int length in axisLength)
                    if (length < 1)
                        throw new ArgumentOutOfRangeException("axisLength");
                Array.Copy(axisLength, sizes, Math.Min(axisLength.Length, dimensions));
                cells.Clear();
            }
        }

        public void SetSizeAxis(int axis, int length) {
            lock (locker) {
                if (axis < 0 || axis >= dimensions)
                    throw new ArgumentOutOfRangeException("axis");
                if (length < 1)
                    throw new ArgumentOutOfRangeException("length");
                int oldSize = sizes[axis];
                sizes[axis] = length;
                if (oldSize != length)
                    cells.Clear(); 
            }
        }

        public Cell GetCell(params int[] coordinates) {
            Cell result;
            if(!cells.TryGetValue(out result, coordinates)) {
                result = new Cell(dimensions, coordinates);
                cells.Add(result, coordinates);
            }
            return result;
        }

        internal Cell GetCell(CoordIndexer key) {
            Cell result;
            var cellDict = cells as IDictionary<CoordIndexer, Cell>;
            if (!cellDict.TryGetValue(key, out result)) {
                result = new Cell(dimensions, key.Coordinates);
                cellDict.Add(key, result);
            }
            return result;
        }

        internal bool HasCell(params int[] coordinates) {
            Cell result;
            // Unconnected cell treat as unexist cell too.
            return cells.TryGetValue(out result, coordinates) && result.Flag != 0;
        }

        public void GenerateMaze(int[] startCoord, int[] endCoord) {
            lock (locker) {
                if (startCoord == null)
                    startCoord = new int[dimensions];
                else {
                    startCoord = startCoord.Clone() as int[];
                    if (startCoord.Length < dimensions)
                        Array.Resize(ref startCoord, dimensions);
                }
                if (endCoord == null)
                    endCoord = sizes.Clone() as int[];
                else {
                    endCoord = endCoord.Clone() as int[];
                    if (endCoord.Length < dimensions)
                        Array.Resize(ref endCoord, dimensions);
                }
                cells.Clear();
                int currentSize = 0, maxSize = 1, currentDirection, currentAxis, i;
                for (i = 0; i < dimensions; i++) {
                    if (startCoord[i] < 0)
                        startCoord[i] = 0;
                    if (startCoord[i] >= sizes[i])
                        startCoord[i] = sizes[i] - 1;
                    if (endCoord[i] < 0)
                        endCoord[i] = 0;
                    if (endCoord[i] >= sizes[i])
                        endCoord[i] = sizes[i] - 1;
                    maxSize *= sizes[i];
                }
                if (random == null)
                    random = new Random();
                var currentCoords = startCoord.Clone() as int[];
                int[] nextCoords;
                var generatedCells = new Stack<Cell>(maxSize);
                var allowedDirections = new List<int>(dimensions * 2);
                Cell cell = GetCell(currentCoords);
                for (currentSize = 1; currentSize < maxSize;) {
                    nextCoords = currentCoords.Clone() as int[];
                    for (i = 0; i < dimensions; i++) {
                        nextCoords[i] = currentCoords[i] + 1;
                        if (currentCoords[i] < sizes[i] - 1 && cell.HasWall(i, false) && !HasCell(nextCoords))
                            allowedDirections.Add(i + 1);
                        nextCoords[i] = currentCoords[i] - 1;
                        if (currentCoords[i] > 0 && cell.HasWall(i, true) && !HasCell(nextCoords))
                            allowedDirections.Add(-i - 1);
                        nextCoords[i] = currentCoords[i];
                    }
                    if (allowedDirections.Count < 1) {
                        if (generatedCells.Count < 1)
                            throw new Exception("No more route at size: " + currentSize + " (max: " + maxSize + ").");
                        cell = generatedCells.Pop();
                        Array.Copy(cell.Coordinates, currentCoords, dimensions);
                    } else {
                        currentDirection = allowedDirections[random.Next(allowedDirections.Count)];
                        currentAxis = Math.Abs(currentDirection) - 1;
                        cell.Connect(currentAxis, currentDirection < 0);
                        generatedCells.Push(cell);
                        // Move to next cell and break the wall to connect to current one.
                        currentCoords[currentAxis] += currentDirection > 0 ? 1 : -1;
                        cell = GetCell(currentCoords);
                        cell.Connect(currentAxis, currentDirection > 0);
                        allowedDirections.Clear();
                        currentSize++;
                    }
                }
            }
        }

        public void ToStream(Stream output) {
            if (output == null)
                throw new ArgumentNullException("output");
            int maxSize = 1;
            foreach (int size in sizes)
                maxSize *= size;
            using (var writer = new BinaryWriter(output)) {
                writer.Write(dimensions);
                foreach (int c in sizes)
                    writer.Write(c);
                for (int i = 0; i < maxSize; i++)
                    writer.Write(GetCell(CoordIndexer.FromIndex(this, i)).Flag);
            }
        }

        public static DefaultMazeGenerator FromStream(Stream input) {
            if (input == null)
                throw new ArgumentNullException("input");
            int i, maxSize = 1;
            DefaultMazeGenerator mgen;
            using (var reader = new BinaryReader(input)) {
                int dimensions = reader.ReadInt32();
                mgen = new DefaultMazeGenerator(dimensions);
                var sizes = new int[dimensions];
                for(i = 0; i < dimensions; i++)
                    maxSize *= sizes[i] = reader.ReadInt32();
                mgen.SetSize(sizes);
                for (i = 0; i < maxSize; i++)
                    mgen.GetCell(CoordIndexer.FromIndex(mgen, i)).Flag = reader.ReadInt32();
            }
            return mgen;
        }
    }
}
