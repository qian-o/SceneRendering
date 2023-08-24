using Silk.NET.Maths;

namespace Core.Models;

public readonly struct BoneInfo
{
    public int Id { get; }

    public Matrix4X4<float> Offset { get; }

    public BoneInfo(int id, Matrix4X4<float> offset)
    {
        Id = id;
        Offset = offset;
    }
}
