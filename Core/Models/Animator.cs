﻿using Core.Helpers;
using Core.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace Core.Models;

public class Animator
{
    protected readonly GL _gl;

    private Animation? currentAnimation;
    private float currentTime;
    private float deltaTime;

    public Matrix4X4<float>[] FinalBoneMatrices { get; }

    public Texture2D FinalBoneMatricesTexture { get; }

    public Animator(GL gl)
    {
        _gl = gl;

        FinalBoneMatrices = new Matrix4X4<float>[ShaderHelper.MAX_BONES];
        Array.Fill(FinalBoneMatrices, Matrix4X4<float>.Identity);

        FinalBoneMatricesTexture = new Texture2D(_gl, GLEnum.Rgba32f, GLEnum.ClampToEdge);
        FinalBoneMatricesTexture.WriteMatrixArray(FinalBoneMatrices);
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

            FinalBoneMatricesTexture.WriteMatrixArray(FinalBoneMatrices);
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

        Matrix4X4<float> globalTransform = nodeTransform * parentTransform;

        if (currentAnimation!.BoneMapping.TryGetValue(nodeName, out BoneInfo boneInfo))
        {
            FinalBoneMatrices[boneInfo.Id] = boneInfo.Offset * globalTransform * currentAnimation.GlobalInverseTransform;
        }

        for (int i = 0; i < node.Children.Length; i++)
        {
            CalculateBoneTransform(node.Children[i], globalTransform);
        }
    }
}
