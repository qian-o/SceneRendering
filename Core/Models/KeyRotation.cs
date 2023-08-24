using Silk.NET.Maths;

namespace Core.Models;

public struct KeyRotation
{
    public float Time;

    public Quaternion<float> Orientation;

    public KeyRotation(float time, Quaternion<float> orientation)
    {
        Time = time;
        Orientation = orientation;
    }
}
