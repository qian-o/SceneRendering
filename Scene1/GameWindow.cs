﻿using Core.Contracts.Windows;
using Core.Elements;
using Core.Helpers;
using ImGuiNET;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
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
    #endregion

    #region Elements
    private Skybox skybox = null!;
    private Plane floor = null!;
    private Cube cube = null!;
    private Plane gaussianBlurFilter = null!;
    #endregion

    #region Colors
    private Vector4D<float> deviation = new(0.0f);
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

        skyboxProgram = new Program(gl);
        skyboxProgram.Attach(skyboxVertex, skyboxFragment);

        textureProgram = new Program(gl);
        textureProgram.Attach(mvpVertex, textureFragment);

        gaussianBlurProgram = new Program(gl);
        gaussianBlurProgram.Attach(mvpVertex, gaussianBlurFragment);

        skybox = new Skybox(gl);

        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX, "Resources/Textures/skybox/right.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 1, "Resources/Textures/skybox/left.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 2, "Resources/Textures/skybox/top.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 3, "Resources/Textures/skybox/bottom.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 4, "Resources/Textures/skybox/front.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 5, "Resources/Textures/skybox/back.jpg");

        cube = new Cube(gl)
        {
            Transform = Matrix4X4.CreateTranslation(0.0f, 0.505f, 0.0f)
        };
        cube.GetDiffuseTex().WriteImage("Resources/Textures/container2.png");

        floor = new Plane(gl, new Vector2D<float>(500.0f))
        {
            Transform = Matrix4X4.CreateScale(1000.0f)
        };
        floor.GetDiffuseTex().WriteImage("Resources/Textures/wood_floor.jpg");

        gaussianBlurFilter = new Plane(gl);
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

            textureProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
            textureProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
            textureProgram.Disable();
        }

        // 滤镜
        {
            {
                gl.Clear(ClearBufferMask.StencilBufferBit);

                gl.StencilFunc(GLEnum.Always, 1, 0xFF);
                gl.StencilMask(0xFF);

                gaussianBlurFilter.Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(45.0f)) * Matrix4X4.CreateTranslation(0.0f, 1.505f, 1.5f);
                gaussianBlurFilter.GetDiffuseTex().WriteColor(Color.Transparent);

                textureProgram.Enable();
                textureProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
                textureProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
                textureProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

                textureProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
                textureProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

                gaussianBlurFilter.Draw(textureProgram);

                textureProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
                textureProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
                textureProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
                textureProgram.Disable();
            }

            gl.StencilFunc(GLEnum.Equal, 1, 0xFF);
            gl.StencilMask(0x00);

            gaussianBlurFilter.Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(90.0f)) * Matrix4X4.CreateScale(2.0f);
            GaussianBlurFilter(gaussianBlurProgram);

            gl.StencilMask(0xFF);
            gl.StencilFunc(GLEnum.Always, 0, 0xFF);
        }
    }

    protected override void RenderImGui(double obj)
    {
        ImGui.Begin("Filter Settings");

        Vector4 color = (Vector4)deviation;
        ImGui.DragFloat4("Filter Deviation", ref color, 0.01f, -1.0f, 1.0f);
        deviation.X = color.X;
        deviation.Y = color.Y;
        deviation.Z = color.Z;
        deviation.W = color.W;
    }

    private void GaussianBlurFilter(Program program)
    {
        byte* image = gl.GetFrameBufferImage(Width, Height);

        gaussianBlurFilter.GetDiffuseTex().WriteImage(image, Width, Height);

        Marshal.FreeHGlobal((IntPtr)image);

        program.Enable();
        program.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
        program.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
        program.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

        program.SetUniform(ShaderHelper.MVP_ViewUniform, Matrix4X4<float>.Identity);
        program.SetUniform(ShaderHelper.MVP_ProjectionUniform, Matrix4X4<float>.Identity);

        program.SetUniform(ShaderHelper.GaussianBlur_RadiusUniform, 5);
        program.SetUniform(ShaderHelper.GaussianBlur_DeviationUniform, deviation);

        gaussianBlurFilter.Draw(program);

        program.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
        program.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
        program.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
        program.Disable();
    }
}
