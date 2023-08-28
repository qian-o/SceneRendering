using Core.Contracts.Elements;
using Core.Helpers;
using Core.Models;
using Core.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public class Skybox : BaseElement
{
    public override Mesh[] Meshes { get; }

    public Skybox(GL gl) : base(gl)
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

        Meshes = new Mesh[] { new Mesh(gl, vertices, indices, new Texture3D(gl, GLEnum.Rgba, GLEnum.Repeat), new Texture3D(gl, GLEnum.Rgba, GLEnum.Repeat)) };
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
        uint position = (uint)program.GetAttrib(ShaderHelper.Skybox_PositionAttrib);

        foreach (Mesh mesh in Meshes)
        {
            _gl.ActiveTexture(GLEnum.Texture0);
            mesh.Diffuse3D!.Enable();

            program.SetUniform(ShaderHelper.Skybox_SkyboxUniform, 0);

            mesh.Draw(position);
        }
    }
}
