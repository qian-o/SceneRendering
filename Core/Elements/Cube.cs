using Core.Contracts.Elements;
using Core.Helpers;
using Core.Models;
using Core.Models.ShaderStructures;
using Core.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public class Cube : BaseElement
{
    public override Mesh[] Meshes { get; }

    public Cube(GL gl) : this(gl, new Vector2D<float>(1.0f, 1.0f))
    {

    }

    public Cube(GL gl, Vector2D<float> textureScale) : base(gl)
    {
        Vertex[] vertices = new Vertex[]
        {
            // Front face
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, 0.0f, 1.0f), new Vector2D<float>(0.0f, 0.0f)),

            // Back face
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, 0.0f, -1.0f), new Vector2D<float>(0.0f, 0.0f)),

            // Left face
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), new Vector3D<float>(-1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),

            // Right face
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(1.0f, 0.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),

            // Bottom face
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), new Vector3D<float>(0.0f, -1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),

            // Top face
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(1.0f, 1.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(1.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(0.0f, 0.0f)),
            new Vertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), new Vector3D<float>(0.0f, 1.0f, 0.0f), new Vector2D<float>(0.0f, 1.0f))
        };
        uint[] indices = vertices.Select((a, b) => (uint)b).ToArray();

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].TexCoords.X *= textureScale.X;
            vertices[i].TexCoords.Y *= textureScale.Y;
        }

        Meshes = new Mesh[] { new Mesh(gl, vertices, indices, new Texture2D(gl, GLEnum.Rgba, GLEnum.Repeat), new Texture2D(gl, GLEnum.Rgba, GLEnum.Repeat)) };
    }

    public Texture2D GetDiffuseTex()
    {
        return Meshes[0].Diffuse2D!;
    }

    public Texture2D GetSpecularTex()
    {
        return Meshes[0].Specular2D!;
    }

    public override void Draw(Program program)
    {
        bool anyMaterial = program.GetUniform(ShaderHelper.Lighting_MaterialUniform) >= 0;

        uint position = (uint)program.GetAttrib(ShaderHelper.MVP_PositionAttrib);
        uint normal = (uint)program.GetAttrib(ShaderHelper.MVP_NormalAttrib);
        uint texCoords = (uint)program.GetAttrib(ShaderHelper.MVP_TexCoordsAttrib);

        program.SetUniform(ShaderHelper.MVP_ModelUniform, Transform);

        foreach (Mesh mesh in Meshes)
        {
            _gl.ActiveTexture(GLEnum.Texture0);
            mesh.Diffuse2D!.Enable();

            if (anyMaterial)
            {
                _gl.ActiveTexture(GLEnum.Texture1);
                mesh.Specular2D!.Enable();

                Material material = new()
                {
                    Diffuse = 0,
                    Specular = 1,
                    Shininess = 64.0f
                };

                program.SetUniform(ShaderHelper.Lighting_MaterialUniform, material);
            }
            else
            {
                program.SetUniform(ShaderHelper.Texture_TexUniform, 0);
            }

            mesh.Draw(position, normal, texCoords);
        }
    }
}
