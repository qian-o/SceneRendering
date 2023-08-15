using Core.Contracts.Windows;
using Core.Elements;
using Core.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Program = Core.Tools.Program;
using Shader = Core.Tools.Shader;

namespace Scene1;

public class GameWindow : Game
{
    private Program program = null!;
    private Skybox skybox = null!;

    protected override void Load()
    {
        gl.Enable(GLEnum.DepthTest);

        gl.Enable(GLEnum.StencilTest);
        gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);

        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        using Shader vs = new(gl, GLEnum.VertexShader, ShaderHelper.GetSkybox_VertShader());
        using Shader fs = new(gl, GLEnum.FragmentShader, ShaderHelper.GetSkybox_FragShader());

        program = new Program(gl);
        program.Attach(vs, fs);

        skybox = new Skybox(gl);

        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX, "Resources/Textures/skybox/right.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 1, "Resources/Textures/skybox/left.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 2, "Resources/Textures/skybox/top.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 3, "Resources/Textures/skybox/bottom.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 4, "Resources/Textures/skybox/front.jpg");
        skybox.GetDiffuseTex().WriteImage(GLEnum.TextureCubeMapPositiveX + 5, "Resources/Textures/skybox/back.jpg");

        camera.Position = new Vector3D<float>(0.0f, 0.0f, 0.0f);
    }

    protected override void Update(double obj)
    {

    }

    protected override void Render(double obj)
    {
        gl.DepthFunc(GLEnum.Lequal);

        program.Enable();
        program.EnableAttrib(ShaderHelper.Skybox_PositionAttrib);

        program.SetUniform(ShaderHelper.Skybox_ViewUniform, camera.View);
        program.SetUniform(ShaderHelper.Skybox_ProjectionUniform, camera.Projection);

        skybox.Draw(program);

        program.DisableAttrib(ShaderHelper.Skybox_PositionAttrib);
        program.Disable();

        gl.DepthFunc(GLEnum.Less);
    }
}
