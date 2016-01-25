using System;
using System.Text;

namespace JLChnToZ.MultiDimensionalArray {
    public class CoordIndexer: IComparable<CoordIndexer>, IEquatable<CoordIndexer>, IFormattable, ICloneable {
        readonly ISizeObject sizeObject;
        readonly int dimensions;
        readonly int[] coord;
        internal bool hasHashCode;
        internal int hashCode;

        public CoordIndexer(ISizeObject sizeObject, params int[] coord) {
            if(sizeObject == null) throw new ArgumentNullException("sizeObject");
            if(coord == null) throw new ArgumentNullException("coord");
            this.dimensions = sizeObject.Dimensions;
            if(dimensions < 1) throw new ArgumentOutOfRangeException("sizeObject", "dimensions of a size object must be greater or equals to 1.");
            this.sizeObject = sizeObject;
            this.coord = new int[dimensions];
            Array.Copy(coord, this.coord, Math.Min(dimensions, coord.Length));
        }

        public static CoordIndexer FromIndex(ISizeObject sizeObject, int index) {
            int dimensions = sizeObject.Dimensions, size = 1, value = 0, i;
            var coords = new int[dimensions];
            for(i = 0; i < dimensions; i++)
                size *= sizeObject.GetSize(i);
            for(i = dimensions - 1; i >= 0; i--) {
                size /= sizeObject.GetSize(i);
                coords[i] = index % size - value;
                value += coords[i];
            }
            return new CoordIndexer(sizeObject, coords);
        }

        public int[] Coordinates {
            get { return coord.Clone() as int[]; }
        }

        public int this[int index] {
            get {
                if(index < 0 || index >= dimensions)
                    throw new ArgumentOutOfRangeException("index");
                return coord[index];
            }
        }

        public int Index {
            get {
                int result = coord[0];
                for(int i = 1, l = dimensions; i < l; i++)
                    result = result * sizeObject.GetSize(i - 1) + coord[i];
                return result;
            }
        }

        public bool IsValid {
            get {
                bool valid = true;
                for(int i = 0; i < dimensions; i++)
                    if(coord[i] < 0 || coord[i] >= sizeObject.GetSize(i)) {
                        valid = false;
                        break;
                    }
                return valid;
            }
        }

        public int CompareTo(CoordIndexer other) {
            return CoordIndexComparer.Instance.Compare(this, other);
        }

        public bool Equals(CoordIndexer other) {
            return CoordIndexComparer.Instance.Equals(this, other);
        }

        public override bool Equals(object obj) {
            var other = obj as CoordIndexer;
            return CoordIndexComparer.Instance.Equals(this, other);
        }

        public override int GetHashCode() {
            return CoordIndexComparer.Instance.GetHashCode(this);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            bool first = true;
            sb.Append("[");
            foreach(int c in coord) {
                if(!first)
                    sb.Append(", ");
                else
                    first = false;
                sb.Append(c);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            var sb = new StringBuilder();
            bool first = true;
            sb.Append("[");
            foreach(int c in coord) {
                if(!first)
                    sb.Append(", ");
                else
                    first = false;
                sb.Append(c.ToString(formatProvider));
            }
            sb.Append("]");
            return sb.ToString();
        }

        public bool IsChild(ISizeObject parent) {
            return parent == sizeObject;
        }

        public object Clone() {
            return new CoordIndexer(sizeObject, coord);
        }
    }
}
