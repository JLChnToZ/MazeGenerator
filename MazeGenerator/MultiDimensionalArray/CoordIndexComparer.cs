using System;
using System.Collections.Generic;

namespace JLChnToZ.MultiDimensionalArray {
    public class CoordIndexComparer: IEqualityComparer<CoordIndexer>, IComparer<CoordIndexer> {
        static CoordIndexComparer instance;
        public static CoordIndexComparer Instance {
            get {
                if(instance == null)
                    instance = new CoordIndexComparer();
                return instance;
            }
        }

        static List<int> primes = new List<int>();
        static object primeWriteLock = new object();
        static int GetPrimeNumber(int index) {
            if(index < 0) throw new ArgumentOutOfRangeException("index");
            bool add;
            lock (primeWriteLock) {
                for(int i = (primes.Count > 0 ? primes[primes.Count - 1] : 1) + 1; primes.Count <= index; i++) {
                    add = true;
                    foreach(int prime in primes)
                        if(i % prime == 0) {
                            add = false;
                            break;
                        }
                    if(add) primes.Add(i);
                }
            }
            return primes[index];
        }

        private CoordIndexComparer() { }

        public int Compare(CoordIndexer x, CoordIndexer y) {
            if(x == null) throw new ArgumentNullException("x");
            if(y == null) throw new ArgumentNullException("y");
            return x.Index.CompareTo(y.Index);
        }

        public bool Equals(CoordIndexer x, CoordIndexer y) {
            if(x == null && y == null) return true;
            if(x == null || y == null) return false;
            return x.Index == y.Index;
        }

        public int GetHashCode(CoordIndexer obj) {
            if(obj == null) throw new ArgumentNullException("obj");
            if(obj.hasHashCode) return obj.hashCode;
            unchecked {
                int hash = 0;
                var coords = obj.Coordinates;
                for(int i = 0, l = coords.Length, temp; i < l; i++) {
                    temp = coords[i].GetHashCode();
                    hash += GetPrimeNumber(i) * ((temp << 1) ^ (temp >> 31) + 1);
                }
                obj.hashCode = hash;
                obj.hasHashCode = true;
                return hash;
            }
        }
    }
}
