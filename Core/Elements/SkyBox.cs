using Core.Contracts.Elements;
using Core.Models;
using Core.Tools;
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
            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            
            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),

            new Vertex(new Vector3D<float>(1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),

            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),

            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f,  1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),

            new Vertex(new Vector3D<float>(-1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f, -1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>(-1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f)),
            new Vertex(new Vector3D<float>( 1.0f, -1.0f,  1.0f), new Vector3D<float>(0.0f), new Vector2D<float>(0.0f))
        };
        uint[] indices = vertices.Select((a, b) => (uint)b).ToArray();

        Meshes = new Mesh[] { new Mesh(gl, vertices, indices, new Texture3D(gl, GLEnum.Rgba8, GLEnum.Repeat), new Texture3D(gl, GLEnum.Rgba8, GLEnum.Repeat)) };
    }

    public Texture3D GetDiffuseTex()
    {
        return Meshes[0].Diffuse3D!;
    }

    public Texture3D GetSpecularTex()
    {
        return Meshes[0].Specular3D!;
    }

    public override void Draw(Program program)
    {

    }
}
