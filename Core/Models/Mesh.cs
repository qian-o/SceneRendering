using Core.Tools;
using Silk.NET.OpenGLES;

namespace Core.Models;

public unsafe class Mesh : IDisposable
{
    private readonly GL _gl;

    public uint VBO { get; }

    public uint EBO { get; }

    public Vertex[] Vertices { get; }

    public uint[] Indices { get; }

    public Texture2D? Diffuse2D { get; }

    public Texture2D? Specular2D { get; }

    public Texture3D? Diffuse3D { get; }

    public Texture3D? Specular3D { get; }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture2D diffuse, Texture2D specular)
    {
        _gl = gl;

        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        Vertices = vertices;
        Indices = indices;

        Diffuse2D = diffuse;
        Specular2D = specular;

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData<Vertex>(GLEnum.ArrayBuffer, (uint)(Vertices.Length * sizeof(Vertex)), null, GLEnum.DynamicDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), null, GLEnum.DynamicDraw);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

        UpdateVertices();
        UpdateIndices();
    }

    public Mesh(GL gl, Vertex[] vertices, uint[] indices, Texture3D diffuse, Texture3D specular)
    {
        _gl = gl;

        VBO = gl.GenBuffer();
        EBO = gl.GenBuffer();

        Vertices = vertices;
        Indices = indices;

        Diffuse3D = diffuse;
        Specular3D = specular;

        gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        gl.BufferData<Vertex>(GLEnum.ArrayBuffer, (uint)(Vertices.Length * sizeof(Vertex)), null, GLEnum.DynamicDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        gl.BufferData<uint>(GLEnum.ElementArrayBuffer, (uint)(Indices.Length * sizeof(uint)), null, GLEnum.DynamicDraw);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

        UpdateVertices();
        UpdateIndices();
    }

    public void UpdateVertices()
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        _gl.BufferSubData<Vertex>(GLEnum.ArrayBuffer, 0, (uint)(Vertices.Length * sizeof(Vertex)), Vertices);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public void UpdateIndices()
    {
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        _gl.BufferSubData<uint>(GLEnum.ElementArrayBuffer, 0, (uint)(Indices.Length * sizeof(uint)), Indices);
        _gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
    }

    public void Draw(uint position, uint? normal = null, uint? texCoords = null, uint? boneIds = null, uint? weights = null)
    {
        _gl.BindBuffer(GLEnum.ArrayBuffer, VBO);
        _gl.VertexAttribPointer(position, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)0);
        if (normal != null)
        {
            _gl.VertexAttribPointer(normal.Value, 3, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(3 * sizeof(float)));
        }
        if (texCoords != null)
        {
            _gl.VertexAttribPointer(texCoords.Value, 2, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(6 * sizeof(float)));
        }
        if (boneIds != null)
        {
            _gl.VertexAttribPointer(boneIds.Value, 4, GLEnum.Int, false, (uint)sizeof(Vertex), (void*)(8 * sizeof(float)));
        }
        if (weights != null)
        {
            _gl.VertexAttribPointer(weights.Value, 4, GLEnum.Float, false, (uint)sizeof(Vertex), (void*)(12 * sizeof(float)));
        }
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.BindBuffer(GLEnum.ElementArrayBuffer, EBO);
        _gl.DrawElements(GLEnum.Triangles, (uint)Indices.Length, GLEnum.UnsignedInt, (void*)0);
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
