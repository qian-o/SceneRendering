using Core.Contracts.Elements;
using Core.Models;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class MikuMikuCustom : BaseElement
{
    public override Mesh[] Meshes { get; }

    public MikuMikuCustom(GL gl, string pmxPath, string? vmdPath = null) : base(gl)
    {

    }

    public override void Draw(Program program)
    {
    }
}
