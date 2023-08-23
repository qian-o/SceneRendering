using Silk.NET.Maths;

namespace Core.Models;

public class BoneData
{
    public int Id { get; }

    public string Name { get; }

    public Matrix4X4<float> Offset { get; }

    public BoneData(int id, string name, Matrix4X4<float> offset)
    {
        Id = id;
        Name = name;
        Offset = offset;
    }
}
