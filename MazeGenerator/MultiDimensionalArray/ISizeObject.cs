namespace JLChnToZ.MultiDimensionalArray {
    public interface ISizeObject {
        int Dimensions { get; }
        int GetSize(int axis);
    }
}
