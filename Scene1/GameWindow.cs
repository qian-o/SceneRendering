using Core.Contracts.Windows;
using Core.Elements;
using Core.Helpers;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;
using Plane = Core.Elements.Plane;
using Program = Core.Tools.Program;
using Shader = Core.Tools.Shader;

namespace Scene1;

public unsafe class GameWindow : Game
{
    #region Programs
    private Program skyboxProgram = null!;
    private Program textureProgram = null!;
    private Program gaussianBlurProgram = null!;
    private Program boneProgram = null!;
    #endregion

    #region Elements
    private Skybox skybox = null!;
    private Plane floor = null!;
    private Cube cube = null!;
    private Custom kafka = null!;
    private Custom ark = null!;
    private Custom animationTest = null!;
    private Plane gaussianBlurFilter1 = null!;
    private Plane gaussianBlurFilter2 = null!;
    #endregion

    #region Gaussian Blur
    private Vector4D<float> deviation = new(0.0f);
    private float noiseIntensity = 0.1f;
    #endregion

    protected override void Load()
    {
        gl.Enable(GLEnum.DepthTest);

        gl.Enable(GLEnum.StencilTest);
        gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);

        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        using Shader skyboxVertex = new(gl, GLEnum.VertexShader, ShaderHelper.GetSkybox_VertexShader());
        using Shader skyboxFragment = new(gl, GLEnum.FragmentShader, ShaderHelper.GetSkybox_FragmentShader());
        using Shader mvpVertex = new(gl, GLEnum.VertexShader, ShaderHelper.GetMVP_VertexShader());
        using Shader textureFragment = new(gl, GLEnum.FragmentShader, ShaderHelper.GetTexture_FragmentShader());
        using Shader gaussianBlurFragment = new(gl, GLEnum.FragmentShader, ShaderHelper.GetGaussianBlur_FragmentShader());
        using Shader boneVertex = new(gl, GLEnum.VertexShader, ShaderHelper.GetBone_VertexShader());

        skyboxProgram = new Program(gl);
        skyboxProgram.Attach(skyboxVertex, skyboxFragment);

        textureProgram = new Program(gl);
        textureProgram.Attach(mvpVertex, textureFragment);

        gaussianBlurProgram = new Program(gl);
        gaussianBlurProgram.Attach(mvpVertex, gaussianBlurFragment);

        boneProgram = new Program(gl);
        boneProgram.Attach(boneVertex, textureFragment);

        skybox = new Skybox(gl);

        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX, "Resources/Textures/skybox/right.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 1, "Resources/Textures/skybox/left.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 2, "Resources/Textures/skybox/top.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 3, "Resources/Textures/skybox/bottom.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 4, "Resources/Textures/skybox/front.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 5, "Resources/Textures/skybox/back.jpg");

        floor = new Plane(gl, new Vector2D<float>(5000.0f))
        {
            Transform = Matrix4X4.CreateScale(10000.0f)
        };
        floor.GetDiffuseTex().WriteImage("Resources/Textures/wood_floor.jpg");

        cube = new Cube(gl)
        {
            Transform = Matrix4X4.CreateTranslation(0.0f, 0.505f, 0.0f)
        };
        cube.GetDiffuseTex().WriteImage("Resources/Textures/container2.png");

        kafka = new Custom(gl, @"Resources/Models/星穹铁道—卡芙卡无外套/kafka.obj")
        {
            Transform = Matrix4X4.CreateTranslation(0.0f, 0.005f, -5.0f)
        };

        ark = new Custom(gl, "Resources/Models/Ark/Ark.obj")
        {
            Transform = Matrix4X4.CreateScale(new Vector3D<float>(0.1f)) * Matrix4X4.CreateTranslation(5.0f, 0.005f, -5.0f)
        };

        animationTest = new Custom(gl, "Resources/Models/Vampire/dancing_vampire.dae")
        // animationTest = new Custom(gl, "Resources/Models/测试动画.fbx")
        {
            Transform = Matrix4X4.CreateScale(new Vector3D<float>(0.1f)) * Matrix4X4.CreateTranslation(-5.0f, 0.005f, -5.0f)
        };
        animationTest.Animator.PlayAnimation(animationTest.Animations[0]);

        gaussianBlurFilter1 = new Plane(gl)
        {
            Transform = Matrix4X4.CreateScale(new Vector3D<float>(2.0f, 1.0f, 1.0f)) * Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(45.0f)) * Matrix4X4.CreateTranslation(0.0f, 1.505f, 1.5f)
        };
        gaussianBlurFilter1.GetDiffuseTex().WriteColor(Color.Transparent);

        gaussianBlurFilter2 = new Plane(gl)
        {
            Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(90.0f)) * Matrix4X4.CreateScale(2.0f)
        };
    }

    protected override void Update(double obj)
    {
    }

    protected override void Render(double obj)
    {
        // 场景渲染
        {
            gl.DepthFunc(GLEnum.Lequal);

            skyboxProgram.Enable();
            skyboxProgram.EnableAttrib(ShaderHelper.Skybox_PositionAttrib);

            skyboxProgram.SetUniform(ShaderHelper.Skybox_ViewUniform, Matrix4X4.CreateTranslation(camera.Position) * camera.View);
            skyboxProgram.SetUniform(ShaderHelper.Skybox_ProjectionUniform, camera.Projection);

            skybox.Draw(skyboxProgram);

            skyboxProgram.DisableAttrib(ShaderHelper.Skybox_PositionAttrib);
            skyboxProgram.Disable();

            gl.DepthFunc(GLEnum.Less);
        }

        // 地板、场景内物品（不包含滤镜）
        {
            textureProgram.Enable();
            textureProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
            textureProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
            textureProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

            textureProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
            textureProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

            floor.Draw(textureProgram);

            cube.Draw(textureProgram);

            kafka.Draw(textureProgram);

            ark.Draw(textureProgram);

            textureProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            textureProgram.Disable();

            boneProgram.Enable();
            boneProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
            boneProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
            boneProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            boneProgram.EnableAttrib(ShaderHelper.Bone_BoneIdsAttrib);
            boneProgram.EnableAttrib(ShaderHelper.Bone_WeightsAttrib);

            boneProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
            boneProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

            // animationTest.Animator.UpdateAnimation((float)obj);
            animationTest.Draw(boneProgram);

            boneProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
            boneProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
            boneProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            boneProgram.DisableAttrib(ShaderHelper.Bone_BoneIdsAttrib);
            boneProgram.DisableAttrib(ShaderHelper.Bone_WeightsAttrib);
            boneProgram.Disable();
        }

        // 滤镜
        {
            GaussianBlurFilter();
        }
    }

    protected override void RenderImGui(double obj)
    {
        ImGui.Begin("Filter Settings");

        Vector4 color = (Vector4)deviation;
        ImGui.DragFloat4("Filter Deviation", ref color, 0.01f, -1.0f, 1.0f);
        deviation = SystemNumericsExtensions.ToGeneric(color);

        ImGui.DragFloat("Filter Noise Intensity", ref noiseIntensity, 0.01f, 0.0f, 1.0f);
    }

    private void GaussianBlurFilter()
    {
        gl.Clear(ClearBufferMask.StencilBufferBit);

        gl.StencilFunc(GLEnum.Always, 1, 0xFF);
        gl.StencilMask(0xFF);

        {
            textureProgram.Enable();
            textureProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
            textureProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
            textureProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

            textureProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
            textureProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

            gaussianBlurFilter1.Draw(textureProgram);

            textureProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            textureProgram.Disable();
        }

        gl.StencilFunc(GLEnum.Equal, 1, 0xFF);
        gl.StencilMask(0x00);

        {
            gaussianBlurFilter2.GetDiffuseTex().WriteFrame(gl, 0, 0, Width, Height);

            gaussianBlurProgram.Enable();
            gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
            gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
            gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

            gaussianBlurProgram.SetUniform(ShaderHelper.MVP_ViewUniform, Matrix4X4<float>.Identity);
            gaussianBlurProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, Matrix4X4<float>.Identity);

            gaussianBlurProgram.SetUniform(ShaderHelper.GaussianBlur_RadiusUniform, 5);
            gaussianBlurProgram.SetUniform(ShaderHelper.GaussianBlur_DeviationUniform, deviation);
            gaussianBlurProgram.SetUniform(ShaderHelper.GaussianBlur_NoiseIntensityUniform, noiseIntensity);

            gaussianBlurFilter2.Draw(gaussianBlurProgram);

            gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
            gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
            gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            gaussianBlurProgram.Disable();
        }

        gl.StencilMask(0xFF);
        gl.StencilFunc(GLEnum.Always, 0, 0xFF);
    }
}