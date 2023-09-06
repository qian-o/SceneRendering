using Silk.NET.Maths;

namespace Core.Helpers;

public static class GLM
{
    public static Vector3D<float> Mix(Vector3D<float> x, Vector3D<float> y, Vector3D<float> a)
    {
        Vector3D<float> result = new(MathHelper.Lerp(x.X, y.X, a.X),
                                     MathHelper.Lerp(x.Y, y.Y, a.Y),
                                     MathHelper.Lerp(x.Z, y.Z, a.Z));
        return result;
    }
}
