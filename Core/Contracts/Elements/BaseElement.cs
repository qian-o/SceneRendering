using Core.Models;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Contracts.Elements;

public abstract class BaseElement : IDisposable
{
    protected readonly GL _gl;

    public abstract Mesh[] Meshes { get; }

    public Matrix4X4<float> Transform { get; set; } = Matrix4X4<float>.Identity;

    protected BaseElement(GL gl)
    {
        _gl = gl;
    }

    public abstract void Draw(Program program, bool useSpecular);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}