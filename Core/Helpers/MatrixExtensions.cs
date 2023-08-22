using Silk.NET.Maths;
using System.Numerics;

namespace Core.Helpers;

public static class MatrixExtensions
{
    public static Matrix4X4<T> Convert<T>(this Matrix4x4 matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
    {
        Matrix4X4<float> result = new(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        return result.As<T>();
    }
}
