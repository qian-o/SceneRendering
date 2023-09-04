using Silk.NET.Maths;

namespace Core.Helpers;

public static class MatrixExtensions
{
    public static Matrix4X4<T> Invert<T>(this Matrix4X4<T> matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Matrix4X4.Invert(matrix, out Matrix4X4<T> result);

        return result;
    }

    public static Quaternion<T> GetRotation<T>(this Matrix3X3<T> matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Matrix3X3.Decompose(matrix, out _, out Quaternion<T> rotation);

        return rotation;
    }
}
