using Silk.NET.Maths;

namespace Core.Models;

public struct KeyScale
{
    public float Time;

    public Vector3D<float> Scale;

    public KeyScale(float time, Vector3D<float> scale)
    {
        Time = time;
        Scale = scale;
    }
}
