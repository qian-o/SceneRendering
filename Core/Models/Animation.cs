using Core.Elements;
using Core.Helpers;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using AssimpAnimation = Silk.NET.Assimp.Animation;

namespace Core.Models;

public unsafe class Animation
{
    private readonly Custom _custom;

    public string Name { get; }

    public float Duration { get; }

    public float TicksPerSecond { get; }

    public AssimpNodeData RootNode { get; }

    public Matrix4X4<float> GlobalInverseTransform { get; }

    public Dictionary<string, BoneInfo> BoneMapping => _custom.BoneMapping;

    public List<Bone> Bones { get; }

    public Animation(string animationPath, Custom custom, int animationIndex)
    {
        _custom = custom;

        Assimp importer = Assimp.GetApi();

        Scene* scene = importer.ImportFile(animationPath, (uint)PostProcessSteps.Triangulate);

        AssimpAnimation* animation = scene->MAnimations[animationIndex];

        Name = animation->MName.AsString;
        Duration = (float)animation->MDuration;
        TicksPerSecond = (float)animation->MTicksPerSecond;
        RootNode = ReadHeirarchyData(scene->MRootNode);
        GlobalInverseTransform = RootNode.Transformation.Invert();
        Bones = ReadMissingBones(animation);
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

    private List<Bone> ReadMissingBones(AssimpAnimation* animation)
    {
        List<Bone> bones = new();

        for (int i = 0; i < animation->MNumChannels; i++)
        {
            NodeAnim* channel = animation->MChannels[i];

            string name = channel->MNodeName.AsString;

            if (!BoneMapping.TryGetValue(name, out BoneInfo boneInfo))
            {
                boneInfo = new BoneInfo(BoneMapping.Count, Matrix4X4<float>.Identity);

                BoneMapping.Add(name, boneInfo);
            }

            bones.Add(new Bone(boneInfo.Id, name, channel));
        }

        return bones;
    }
}
