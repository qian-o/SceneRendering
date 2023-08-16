using Core.Contracts.Windows;
using Core.Elements;
using Core.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Plane = Core.Elements.Plane;
using Program = Core.Tools.Program;
using Shader = Core.Tools.Shader;

namespace Scene1;

public class GameWindow : Game
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
    private Plane gaussianBlurGlass = null!;
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

        gaussianBlurGlass = new Plane(gl)
        {
            Transform = Matrix4X4.CreateRotationX(MathHelper.DegreesToRadians(90.0f)) * Matrix4X4.CreateTranslation(0.0f, 0.505f, 1.5f)
        };
        gaussianBlurGlass.GetDiffuseTex().WriteColor(new Vector4D<byte>(255, 255, 255, 125));
    }

    protected override void Update(double obj)
    {

    }

    protected override void Render(double obj)
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

        textureProgram.Enable();
        textureProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
        textureProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
        textureProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

        textureProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
        textureProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

        floor.Draw(textureProgram);

        cube.Draw(textureProgram);

        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
        gaussianBlurProgram.Disable();

        gaussianBlurProgram.Enable();
        gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_PositionAttrib);
        gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_NormalAttrib);
        gaussianBlurProgram.EnableAttrib(ShaderHelper.MVP_TexCoordsAttrib);

        gaussianBlurProgram.SetUniform(ShaderHelper.MVP_ViewUniform, camera.View);
        gaussianBlurProgram.SetUniform(ShaderHelper.MVP_ProjectionUniform, camera.Projection);

        gaussianBlurProgram.SetUniform(ShaderHelper.GaussianBlur_RadiusUniform, 30);
        gaussianBlurProgram.SetUniform(ShaderHelper.GaussianBlur_TotalWeightUniform, ShaderHelper.GetTotalWeight(30));

        gaussianBlurGlass.Draw(gaussianBlurProgram);

        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_PositionAttrib);
        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_NormalAttrib);
        gaussianBlurProgram.DisableAttrib(ShaderHelper.MVP_TexCoordsAttrib);
        gaussianBlurProgram.Disable();
    }
}
