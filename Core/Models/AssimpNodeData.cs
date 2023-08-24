using Silk.NET.Maths;

namespace Core.Models;

public struct AssimpNodeData
{
    public string Name;

    public Matrix4X4<float> Transformation;

    public AssimpNodeData[] Children;
}