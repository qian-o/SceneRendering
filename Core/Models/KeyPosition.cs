using Silk.NET.Maths;

namespace Core.Models;

public struct KeyPosition
{
    public float Time;

    public Vector3D<float> Position;

    public KeyPosition(float time, Vector3D<float> position)
    {
        Time = time;
        Position = position;
    }
}
