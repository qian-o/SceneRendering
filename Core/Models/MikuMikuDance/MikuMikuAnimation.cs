using Core.Elements;
using Core.Helpers;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using File = System.IO.File;

namespace Core.Models.MikuMikuDance;

public unsafe class MikuMikuAnimation
{
    public const float TicksPerSecond = 30.0f;

    private readonly MikuMikuCustom _mikuMikuDance;

    public VmdMotion Motion { get; }

    public string Name => Motion.Name;

    public float Duration { get; }

    public AssimpNodeData RootNode { get; }

    public Matrix4X4<float> GlobalInverseTransform { get; }

    public Dictionary<string, BoneInfo> BoneMapping => _mikuMikuDance.BoneMapping;

    public List<MikuMikuBone> Bones { get; } = new List<MikuMikuBone>();

    public MikuMikuAnimation(MikuMikuCustom mikuMikuDance, Scene* scene, string vmdPath)
    {
        _mikuMikuDance = mikuMikuDance;

        Motion = LoadFromStream(File.OpenRead(vmdPath));

        Duration = Motion.BoneFrames[^1].Frame / TicksPerSecond;
        RootNode = ReadHeirarchyData(scene->MRootNode);
        GlobalInverseTransform = RootNode.Transformation.Invert();
        Bones = ReadMissingBones();
    }

    private AssimpNodeData ReadHeirarchyData(Node* src)
    {
        AssimpNodeData dest = new()
        {
            Name = src->MName.AsString,
            Transformation = Matrix4X4.Transpose(src->MTransformation.ToGeneric()),
            Children = new AssimpNodeData[src->MNumChildren]
        };

        for (int i = 0; i < src->MNumChildren; i++)
        {
            dest.Children[i] = ReadHeirarchyData(src->MChildren[i]);
        }

        return dest;
    }

    private List<MikuMikuBone> ReadMissingBones()
    {
        List<MikuMikuBone> bones = new();

        string[] boneNames = Motion.BoneFrames.Select(x => x.Name).Distinct().ToArray();
        foreach (string name in boneNames)
        {
            if (!BoneMapping.TryGetValue(name, out BoneInfo boneInfo))
            {
                boneInfo = new BoneInfo(BoneMapping.Count, Matrix4X4<float>.Identity);

                BoneMapping.Add(name, boneInfo);
            }

            VmdBoneFrame[] vmdBoneFrames = Motion.BoneFrames.Where(item => item.Name == name).ToArray();

            bones.Add(new MikuMikuBone(boneInfo.Id, name, vmdBoneFrames));
        }

        return bones;
    }

    public static VmdMotion LoadFromStream(Stream stream)
    {
        byte[] buffer = new byte[30];
        stream.Read(buffer, 0, buffer.Length);

        if (!buffer.Decode(EncodingType.ASCII).Contains("Vocaloid Motion Data"))
        {
            throw new Exception("Invalid VMD file.");
        }

        int version = BitConverter.ToInt32(buffer, 20);

        buffer = new byte[20];
        stream.Read(buffer, 0, buffer.Length);

        VmdMotion motion = new(buffer.Decode(EncodingType.UTF8), version);

        int bone_frame_num;
        stream.Read(buffer, 0, sizeof(int));
        bone_frame_num = BitConverter.ToInt32(buffer, 0);
        for (int i = 0; i < bone_frame_num; i++)
        {
            motion.BoneFrames.Add(new VmdBoneFrame(stream));
        }

        int face_frame_num;
        stream.Read(buffer, 0, sizeof(int));
        face_frame_num = BitConverter.ToInt32(buffer, 0);
        for (int i = 0; i < face_frame_num; i++)
        {
            motion.FaceFrames.Add(new VmdFaceFrame(stream));
        }

        int camera_frame_num;
        stream.Read(buffer, 0, sizeof(int));
        camera_frame_num = BitConverter.ToInt32(buffer, 0);
        for (int i = 0; i < camera_frame_num; i++)
        {
            motion.CameraFrames.Add(new VmdCameraFrame(stream));
        }

        int light_frame_num;
        stream.Read(buffer, 0, sizeof(int));
        light_frame_num = BitConverter.ToInt32(buffer, 0);
        for (int i = 0; i < light_frame_num; i++)
        {
            motion.LightFrames.Add(new VmdLightFrame(stream));
        }

        // unknown2
        stream.Position += 4;

        if (stream.Position < stream.Length)
        {
            int ik_frame_num;
            stream.Read(buffer, 0, sizeof(int));
            ik_frame_num = BitConverter.ToInt32(buffer, 0);
            for (int i = 0; i < ik_frame_num; i++)
            {
                motion.IkFrames.Add(new VmdIkFrame(stream));
            }
        }

        if (stream.Position != stream.Length)
        {
            throw new Exception("Invalid VMD file.");
        }

        return motion;
    }
}