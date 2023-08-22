using Silk.NET.Maths;

namespace Core.Models;

public struct BoneData
{
    public int Id;

    public Matrix4X4<float> Offset;

    public BoneData(int id, Matrix4X4<float> offset)
    {
        Id = id;
        Offset = offset;
    }
}
