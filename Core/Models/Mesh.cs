using Core.Tools;
using Silk.NET.OpenGLES;

namespace Core.Models;

public unsafe class Mesh : IDisposable
{
    private readonly GL _gl;

    public uint VBO { get; }

    public uint EBO { get; }

    public uint VertexCount { get; }

    public uint IndexCount { get; }

    public Texture2D? Diffuse2D { get; }

    public Texture2D? Specular2D { get; }

    public Texture3D? Diffuse3D { get; }

    public Texture3D? Specular3D { get; }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture2D diffuse, Texture2D specular)
    {
        _gl = gl;

        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        VertexCount = (uint)vertices.Length;
        IndexCount = (uint)indices.Length;

        Diffuse2D = diffuse;
        Specular2D = specular;

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData<Vertex>(GLEnum.ArrayBuffer, VertexCount * (uint)sizeof(Vertex), vertices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, IndexCount * sizeof(uint), indices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture3D diffuse, Texture3D specular)
    {
        _gl = gl;

        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        VertexCount = (uint)vertices.Length;
        IndexCount = (uint)indices.Length;

        Diffuse3D = diffuse;
        Specular3D = specular;

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData<Vertex>(GLEnum.ArrayBuffer, VertexCount * (uint)sizeof(Vertex) * sizeof(float), vertices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, IndexCount * sizeof(uint), indices, GLEnum.StaticDraw);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Draw(uint position, uint? normal = null, uint? texCoords = null)
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        _gl.VertexAttribPointer(position, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)0);
        if (normal != null)
        {
            _gl.VertexAttribPointer(normal.Value, 3, GLEnum.Float, false, 8 * sizeof(float), (void*)(3 * sizeof(float)));
        }
        if (texCoords != null)
        {
            _gl.VertexAttribPointer(texCoords.Value, 2, GLEnum.Float, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
        }
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        _gl.DrawElements(GLEnum.Triangles, VertexCount, GLEnum.UnsignedInt, (void*)0);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(VBO);
        _gl.DeleteBuffer(EBO);

        Diffuse2D?.Dispose();
        Specular2D?.Dispose();

        Diffuse3D?.Dispose();
        Specular3D?.Dispose();

        GC.SuppressFinalize(this);
    }
}
