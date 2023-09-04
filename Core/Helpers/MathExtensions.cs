﻿using Silk.NET.Maths;

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
}
