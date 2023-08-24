using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models;

public unsafe struct Vertex
{
    public Vector3D<float> Position;

    public Vector3D<float> Normal;

    public Vector2D<float> TexCoords;

    public Vector3D<float> Tangent;

    public Vector3D<float> Bitangent;

    public fixed int BoneIds[ShaderHelper.MAX_BONE_INFLUENCE];

    public fixed float BoneWeights[ShaderHelper.MAX_BONE_INFLUENCE];

    public Vertex()
    {
        for (int i = 0; i < ShaderHelper.MAX_BONE_INFLUENCE; i++)
        {
            BoneIds[i] = -1;
            BoneWeights[i] = 0.0f;
        }
    }

    public Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords) : this()
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
    }
}
