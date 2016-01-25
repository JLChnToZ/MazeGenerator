using System;
using System.Collections.Generic;

namespace JLChnToZ.MazeGenerator {
    static class HelperFunctions {
        public static int SetBit(this int source, int index, bool isOn) {
            return isOn ? (source | (1 << index)) : (source & ~(1 << index));
        }

        public static long SetBit(this long source, int index, bool isOn) {
            return isOn ? (source | (1L << index)) : (source & ~(1L << index));
        }

        public static uint SetBit(this uint source, int index, bool isOn) {
            return isOn ? (source | (1U << index)) : (source & ~(1U << index));
        }

        public static ulong SetBit(this ulong source, int index, bool isOn) {
            return isOn ? (source | (1UL << index)) : (source & ~(1UL << index));
        }

        public static bool HasBit(this int source, int index) {
            return (source & (1 << index)) != 0;
        }

        public static bool HasBit(this long source, int index) {
            return (source & (1L << index)) != 0L;
        }

        public static bool HasBit(this uint source, int index) {
            return (source & (1U << index)) != 0U;
        }

        public static bool HasBit(this ulong source, int index) {
            return (source & (1UL << index)) != 0UL;
        }
    }
}
