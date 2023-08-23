using Silk.NET.Maths;

namespace Core.Models;

public class AnimationNode
{
    public string Name { get; }

    public Vector3D<float> Position { get; }

    public Quaternion<float> Rotation { get; }

    public Vector3D<float> Scale { get; }

    public AnimationNode(string name, Vector3D<float> position, Quaternion<float> rotation, Vector3D<float> scale)
    {
        Name = name;
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
}
