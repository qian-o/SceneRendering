using Core.Contracts.Elements;
using Core.Models;
using Core.Models.MikuMikuDance;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class MikuMikuModel : BaseElement
{
    private readonly PMXFile _pmx;

    public override Mesh[] Meshes { get; }

    public MikuMikuModel(GL gl, string pmxPath, string? vmdPath = null) : base(gl)
    {
        _pmx = new PMXFile(pmxPath);

        Meshes = Array.Empty<Mesh>();
    }

    public override void Draw(Program program)
    {
    }
}
