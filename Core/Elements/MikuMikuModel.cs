using Core.Contracts.Elements;
using Core.Models;
using Core.Models.MikuMikuDance.PMX;
using Core.Models.MikuMikuDance.VMD;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class MikuMikuModel : BaseElement
{
    private readonly PMXFile _pmx;
    private readonly VMDFile? _vmd;

    public override Mesh[] Meshes { get; }

    public MikuMikuModel(GL gl, string pmxPath, string? vmdPath = null) : base(gl)
    {
        _pmx = new PMXFile(pmxPath);

        if (vmdPath != null)
        {
            _vmd = new VMDFile(vmdPath);
        }

        Meshes = Array.Empty<Mesh>();
    }

    public override void Draw(Program program)
    {
    }
}
