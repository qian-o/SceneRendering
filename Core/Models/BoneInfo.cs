using Silk.NET.Maths;

namespace Core.Models;

public struct BoneInfo
{
    public int Id;

    public Matrix4X4<float> Offset;

    public BoneInfo(int id, Matrix4X4<float> offset)
    {
        Id = id;
        Offset = offset;
    }
}
