using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models;

public class Animator
{
    private Animation? currentAnimation;
    private float currentTime;
    private float deltaTime;

    public Matrix4X4<float>[] FinalBoneMatrices { get; }

    public Animator()
    {
        FinalBoneMatrices = new Matrix4X4<float>[ShaderHelper.MAX_BONES];

        Array.Fill(FinalBoneMatrices, Matrix4X4<float>.Identity);
    }

    public void PlayAnimation(Animation animation)
    {
        currentAnimation = animation;
        currentTime = 0.0f;
    }

    public void UpdateAnimation(float dt)
    {
        deltaTime = dt;

        if (currentAnimation != null)
        {
            currentTime += currentAnimation.TicksPerSecond * deltaTime;
            currentTime %= currentAnimation.Duration;

            CalculateBoneTransform(currentAnimation.RootNode, Matrix4X4<float>.Identity);
        }
    }

    private void CalculateBoneTransform(AssimpNodeData node, Matrix4X4<float> parentTransform)
    {
        string nodeName = node.Name;
        Matrix4X4<float> nodeTransform = node.Transformation;

        if (currentAnimation!.Bones.Find(item => item.Name == nodeName) is Bone bone)
        {
            bone.Update(currentTime);

            nodeTransform = bone.LocalTransform;
        }

        Matrix4X4<float> globalTransform = parentTransform * nodeTransform;

        if (currentAnimation!.BoneMapping.TryGetValue(nodeName, out BoneInfo boneInfo))
        {
            FinalBoneMatrices[boneInfo.Id] = globalTransform * boneInfo.Offset;
        }

        for (int i = 0; i < node.Children.Length; i++)
        {
            CalculateBoneTransform(node.Children[i], globalTransform);
        }
    }
}
