using Silk.NET.Maths;
using BtMatrix4x4 = Evergine.Mathematics.Matrix4x4;
using BtVector3 = Evergine.Mathematics.Vector3;

namespace Core.Helpers;

public static class MathExtensions
{
    public static Vector3D<float> ToVector3D(this Vector4D<float> vector)
    {
        return new Vector3D<float>(vector.X, vector.Y, vector.Z);
    }

    public static Vector3D<float> ToVector3D(this Quaternion<float> quaternion)
    {
        return new Vector3D<float>(quaternion.X, quaternion.Y, quaternion.Z);
    }

    public static Vector4D<float> ToVector4D(this Vector3D<float> vector)
    {
        return new Vector4D<float>(vector.X, vector.Y, vector.Z, 0);
    }

    public static Vector4D<float> ToVector4D(this Vector3D<float> vector, float w)
    {
        return new Vector4D<float>(vector.X, vector.Y, vector.Z, w);
    }

    public static Vector4D<float> ToVector4D(this Quaternion<float> quaternion)
    {
        return new Vector4D<float>(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }

    public static BtVector3 ToBulletVector3(this Vector3D<float> vector)
    {
        return new BtVector3(vector.X, vector.Y, vector.Z);
    }

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

    public static Matrix4X4<float> InvZ(this Matrix4X4<float> matrix)
    {
        Matrix4X4<float> invZ = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, -1.0f));

        return invZ * matrix * invZ;
    }

    public static BtMatrix4x4 ToBulletMatrix4x4(this Matrix4X4<float> matrix)
    {
        return new BtMatrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                               matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                               matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                               matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }

    public static Matrix4X4<float> ToMatrix(this BtMatrix4x4 matrix)
    {
        return new Matrix4X4<float>(matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                                    matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                                    matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                                    matrix.M41, matrix.M42, matrix.M43, matrix.M44);
    }
}
