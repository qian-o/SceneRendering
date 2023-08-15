using Core.Contracts.Elements;
using Core.Models;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public class SkyBox : BaseElement
{
    public override Mesh[] Meshes { get; }

    public SkyBox(GL gl) : base(gl)
    {
        Vertex[] vertices = new Vertex[]
        {
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f,  0.0f, -1.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f,  0.5f, -0.5f), new Vector3D<float>(0.0f,  0.0f, -1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f,  0.5f, -0.5f), new Vector3D<float>(0.0f,  0.0f, -1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f,  0.5f, -0.5f), new Vector3D<float>(0.0f,  0.0f, -1.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f,  0.0f, -1.0f), new Vector2D<float>(0.0f, 0.0f)),
        };
        uint[] indices = vertices.Select((a, b) => (uint)b).ToArray();
    }

    public override void Draw(Program program)
    {
        throw new NotImplementedException();
    }
}
