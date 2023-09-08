using Core.Contracts.Elements;
using Core.Models;
using Core.Models.MikuMikuDance.PMX;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;

namespace Core.Elements;

public unsafe class MikuMikuModel : BaseElement
{
    public override Mesh[] Meshes { get; }

    public MikuMikuModel(GL gl, string pmxPath, string? vmdPath = null, string? vpdPath = null) : base(gl)
    {
        PMXModel model = new();

        model.Load(pmxPath, "Resources/Textures");

        if (vmdPath != null)
        {
            //model.InitializeAnimation();

            //VMDAnimation animation = new();
            //animation.Create(model);

            //VMDFile vmd = new(vmdPath);
            //animation.Add(vmd);

            //animation.SyncPhysics(0.0f);
        }

        Meshes = Array.Empty<Mesh>();
    }

    public override void Draw(Program program)
    {
    }
}
