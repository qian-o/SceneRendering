using Core.Contracts.Windows;
using Core.Elements;
using Core.Helpers;
using Core.Tools;
using Silk.NET.OpenGLES;
using System.Drawing;
using Program = Core.Tools.Program;
using Shader = Core.Tools.Shader;

namespace Scene1;

public class GameWindow : Game
{
    private Program program = null!;
    private Cube cube = null!;

    protected override void Load()
    {
        gl.Enable(GLEnum.DepthTest);

        gl.Enable(GLEnum.StencilTest);
        gl.StencilOp(GLEnum.Keep, GLEnum.Keep, GLEnum.Replace);

        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        gl.ClearColor(Color.CornflowerBlue);

        using Shader vs = new(gl, GLEnum.VertexShader, ShaderHelper.GetModelViewProjectionShader());
        using Shader fs = new(gl, GLEnum.FragmentShader, ShaderHelper.GetTextureShader());

        program = new Program(gl);
        program.Attach(vs, fs);

        cube = new Cube(gl);

        cube.GetDiffuseTex().WriteColor(Color.Red);
    }

    protected override void Update(double obj)
    {

    }

    protected override void Render(double obj)
    {
        program.Enable();

        program.EnableAttrib(ShaderHelper.PositionAttrib);
        program.EnableAttrib(ShaderHelper.NormalAttrib);
        program.EnableAttrib(ShaderHelper.TexCoordsAttrib);

        program.SetUniform(ShaderHelper.ModelUniform, cube.Transform);
        program.SetUniform(ShaderHelper.ViewUniform, camera.View);
        program.SetUniform(ShaderHelper.ProjectionUniform, camera.Projection);

        cube.Draw(program, false);

        program.DisableAttrib(ShaderHelper.PositionAttrib);
        program.DisableAttrib(ShaderHelper.NormalAttrib);
        program.DisableAttrib(ShaderHelper.TexCoordsAttrib);

        program.Disable();
    }
}
