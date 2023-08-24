using Silk.NET.Maths;

namespace Core.Helpers;

public static class AssimpGLMHelper
{
    public static Quaternion<float> GetGLMQuat(this Quaternion<float> from)
    {
        return new Quaternion<float>(from.X, from.Y, from.Z, from.W);
    }
}
