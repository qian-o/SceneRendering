using Core.Contracts.Elements;
using Core.Models;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class MikuMikuModel : BaseElement
{
    public override Mesh[] Meshes { get; }

    public MikuMikuModel(GL gl, string pmxPath, string? vmdPath = null) : base(gl)
    {
        Meshes = Array.Empty<Mesh>();
    }

    public override void Draw(Program program)
    {
    }
}
