using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JLChnToZ.MultiDimensionalArray {
    public class Array<T>: ISizeObject, IList<T>, IDictionary<CoordIndexer, T>, ICloneable {
        readonly int dimension;
        readonly int[] sizes;
        readonly Dictionary<CoordIndexer, T> container;

        public Array(params int[] sizes) {
            if(sizes == null || sizes.Length < 1) throw new ArgumentException("Size is required.");
            dimension = sizes.Length;
            this.sizes = sizes.Clone() as int[];
            container = new Dictionary<CoordIndexer, T>(CoordIndexComparer.Instance);
        }

        public int Dimensions {
            get { return dimension; }
        }

        public int Count {
            get { return container.Count; }
        }

        public int Size {
            get {
                int result = 1;
                foreach(int size in sizes)
                    result *= size;
                return result;
            }
        }

        ICollection<CoordIndexer> IDictionary<CoordIndexer, T>.Keys {
            get { return container.Keys; }
        }

        public ICollection<T> Values {
            get { return new ReadOnlyCollection<T>(this); }
        }

        bool ICollection<T>.IsReadOnly {
            get { return true; }
        }

        bool ICollection<KeyValuePair<CoordIndexer, T>>.IsReadOnly {
            get { return false; }
        }

        public T this[int index] {
            get {
                T value;
                container.TryGetValue(CoordIndexer.FromIndex(this, index), out value);
                return value;
            }
            set {
                container[CoordIndexer.FromIndex(this, index)] = value;
            }
        }

        T IDictionary<CoordIndexer, T>.this[CoordIndexer key] {
            get {
                T value;
                container.TryGetValue(key, out value);
                return value;
            }
            set {
                EnsureIndexer(key);
                container[key] = value;
            }
        }

        public bool TryGetValue(out T value, params int[] indeces) {
            EnsureIndeces(indeces, true);
            return container.TryGetValue(new CoordIndexer(this, indeces), out value);
        }

        bool IDictionary<CoordIndexer, T>.TryGetValue(CoordIndexer key, out T value) {
            EnsureIndexer(key);
            return container.TryGetValue(key, out value);
        }

        public T GetValue(params int[] indeces) {
            EnsureIndeces(indeces, true);
            T value;
            container.TryGetValue(new CoordIndexer(this, indeces), out value);
            return value;
        }

        public void SetValue(T value, params int[] indeces) {
            EnsureIndeces(indeces, true);
            container[new CoordIndexer(this, indeces)] = value;
        }

        public int GetSize(int axis) {
            return sizes[axis];
        }

        public void Resize(params int[] newSizes) {
            EnsureIndeces(newSizes, false);
            Array.Copy(newSizes, sizes, dimension);
        }

        public void ResizeAxis(int axis, int newSize) {
            if(axis < 0 || axis >= dimension) throw new ArgumentOutOfRangeException("axis");
            sizes[axis] = newSize;
        }

        public void Add(T value, params int[] indeces) {
            EnsureIndeces(indeces, true);
            container.Add(new CoordIndexer(this, indeces), value);
        }

        void IDictionary<CoordIndexer, T>.Add(CoordIndexer key, T value) {
            EnsureIndexer(key);
            container.Add(key, value);
        }

        void ICollection<KeyValuePair<CoordIndexer, T>>.Add(KeyValuePair<CoordIndexer, T> item) {
            EnsureIndexer(item.Key);
            container.Add(item.Key, item.Value);
        }

        void ICollection<T>.Add(T item) {
            throw new NotSupportedException();
        }

        void IList<T>.Insert(int index, T item) {
            throw new NotSupportedException();
        }

        public bool Contains(T item) {
            return container.ContainsValue(item);
        }

        bool ICollection<KeyValuePair<CoordIndexer, T>>.Contains(KeyValuePair<CoordIndexer, T> item) {
            return (container as IDictionary<CoordIndexer, T>).Contains(item);
        }

        bool IDictionary<CoordIndexer, T>.ContainsKey(CoordIndexer key) {
            EnsureIndexer(key);
            return container.ContainsKey(key);
        }

        public int IndexOf(T item) {
            var comparer = EqualityComparer<T>.Default;
            foreach(var kv in container)
                if(comparer.Equals(kv.Value, item))
                    return kv.Key.Index;
            return -1;
        }

        public bool Remove(params int[] indeces) {
            EnsureIndeces(indeces, true);
            return container.Remove(new CoordIndexer(this, indeces));
        }

        public void Clean() {
            var cleanUp = new HashSet<CoordIndexer>();
            foreach(var coord in container.Keys)
                if(!coord.IsValid) cleanUp.Add(coord);
            foreach(var coord in cleanUp)
                container.Remove(coord);
        }

        bool IDictionary<CoordIndexer, T>.Remove(CoordIndexer key) {
            EnsureIndexer(key);
            return container.Remove(key);
        }

        bool ICollection<KeyValuePair<CoordIndexer, T>>.Remove(KeyValuePair<CoordIndexer, T> item) {
            EnsureIndexer(item.Key);
            return container.Remove(item.Key);
        }

        bool ICollection<T>.Remove(T item) {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index) {
            throw new NotSupportedException();
        }

        public void Clear() {
            container.Clear();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            foreach(var item in this) {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        public void CopyTo(KeyValuePair<CoordIndexer, T>[] array, int arrayIndex) {
            (container as IDictionary<CoordIndexer, T>).CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() {
            T value;
            for(int i = 0, size = Size; i < size; i++) {
                container.TryGetValue(CoordIndexer.FromIndex(this, i), out value);
                yield return value;
            }
        }

        IEnumerator<KeyValuePair<CoordIndexer, T>> IEnumerable<KeyValuePair<CoordIndexer, T>>.GetEnumerator() {
            CoordIndexer idx;
            T value;
            for(int i = 0, l = Size; i < l; i++) {
                idx = CoordIndexer.FromIndex(this, i);
                container.TryGetValue(idx, out value);
                yield return new KeyValuePair<CoordIndexer, T>(idx, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void EnsureIndexer(CoordIndexer indexer) {
            if(indexer == null)
                throw new ArgumentNullException("indexer");
            if(!indexer.IsChild(this))
                throw new ArgumentException("The coord indexer instance is not initialized for this array.", "key");
        }

        void EnsureIndeces(int[] indeces, bool checkBoundary) {
            if(indeces == null)
                throw new ArgumentNullException("indeces");
            if(indeces.Length < dimension)
                throw new ArgumentException("Parameters are not enough to indicate an index.", "indeces");
            if(!checkBoundary) return;
            for(int i = 0, l = sizes.Length; i < l; i++)
                if(indeces[i] < 0 || indeces[i] >= sizes[i])
                    throw new ArgumentOutOfRangeException("indeces", "Value of index " + i + " is out of range.");
        }

        public object Clone() {
            var cloned = new Array<T>(sizes);
            foreach(var kv in container)
                cloned.container.Add(new CoordIndexer(cloned, kv.Key.Coordinates), kv.Value);
            return cloned;
        }
    }
}
