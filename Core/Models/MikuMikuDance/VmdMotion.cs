namespace Core.Models.MikuMikuDance;

public class VmdMotion
{
    public string Name { get; }

    public int Version { get; }

    public List<VmdBoneFrame> BoneFrames { get; } = new List<VmdBoneFrame>();

    public List<VmdFaceFrame> FaceFrames { get; } = new List<VmdFaceFrame>();

    public List<VmdCameraFrame> CameraFrames { get; } = new List<VmdCameraFrame>();

    public List<VmdLightFrame> LightFrames { get; } = new List<VmdLightFrame>();

    public List<VmdIkFrame> IkFrames { get; } = new List<VmdIkFrame>();

    public VmdMotion(string name, int version)
    {
        Name = name;
        Version = version;
    }
}
