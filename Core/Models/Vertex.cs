using Silk.NET.Maths;

namespace Core.Models;

public unsafe struct Vertex
{
    public const int MAX_BONE_INFLUENCE = 4;

    public Vector3D<float> Position;

    public Vector3D<float> Normal;

    public Vector2D<float> TexCoords;

    public fixed int BoneIds[4];

    public fixed float BoneWeights[4];

    public Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
    }
}
