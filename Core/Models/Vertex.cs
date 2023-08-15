using Silk.NET.Maths;

namespace Core.Models;

public struct Vertex
{
    public Vector3D<float> Position;

    public Vector3D<float> Normal;

    public Vector2D<float> TexCoords;

    public Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoords)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
    }
}
