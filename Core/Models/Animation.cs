using Core.Elements;
using Silk.NET.Assimp;
using Silk.NET.Maths;
using AssimpAnimation = Silk.NET.Assimp.Animation;

namespace Core.Models;

public unsafe class Animation
{
    private readonly Custom _custom;

    public float Duration { get; }

    public float TicksPerSecond { get; }

    public AssimpNodeData RootNode { get; }

    public Dictionary<string, BoneInfo> BoneMapping => _custom.BoneMapping;

    public List<Bone> Bones { get; }

    public Animation(string animationPath, Custom custom, int animationIndex)
    {
        _custom = custom;
        Bones = new List<Bone>();

        Assimp importer = Assimp.GetApi();

        Scene* scene = importer.ImportFile(animationPath, (uint)PostProcessSteps.Triangulate);

        AssimpAnimation* animation = scene->MAnimations[animationIndex];

        Duration = (float)animation->MDuration;
        TicksPerSecond = (float)animation->MTicksPerSecond;

        ReadMissingBones(animation);

        AssimpNodeData assimpNodeData = new();
        ReadHeirarchyData(ref assimpNodeData, scene->MRootNode);

        RootNode = assimpNodeData;
    }

    private void ReadMissingBones(AssimpAnimation* animation)
    {
        for (int i = 0; i < animation->MNumChannels; i++)
        {
            NodeAnim* channel = animation->MChannels[i];

            string name = channel->MNodeName.AsString;

            if (!BoneMapping.TryGetValue(name, out BoneInfo boneInfo))
            {
                boneInfo = new BoneInfo(BoneMapping.Count, Matrix4X4<float>.Identity);

                BoneMapping.Add(name, boneInfo);
            }

            Bones.Add(new Bone(boneInfo.Id, name, channel));
        }
    }

    private void ReadHeirarchyData(ref AssimpNodeData dest, Node* src)
    {
        dest.Name = src->MName.AsString;
        dest.Transformation = src->MTransformation.ToGeneric();
        dest.Children = new AssimpNodeData[src->MNumChildren];

        for (int i = 0; i < src->MNumChildren; i++)
        {
            AssimpNodeData newData = new();
            ReadHeirarchyData(ref newData, src->MChildren[i]);

            dest.Children[i] = newData;
        }
    }
}
