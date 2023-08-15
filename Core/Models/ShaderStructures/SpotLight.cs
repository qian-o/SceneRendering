using Core.Contracts.Models.ShaderStructures;
using Core.Tools;
using Silk.NET.Maths;

namespace Core.Models.ShaderStructures;

public struct SpotLight : IStructure
{
    public Vector3D<float> Position;

    public Vector3D<float> Direction;

    public float CutOff;

    public float OuterCutOff;

    public float Constant;

    public float Linear;

    public float Quadratic;

    public Vector3D<float> Ambient;

    public Vector3D<float> Diffuse;

    public Vector3D<float> Specular;

    public readonly void Enable(string name, Program program)
    {
        program.SetUniform($"{name}.position", Position);
        program.SetUniform($"{name}.direction", Direction);
        program.SetUniform($"{name}.cutOff", CutOff);
        program.SetUniform($"{name}.outerCutOff", OuterCutOff);
        program.SetUniform($"{name}.constant", Constant);
        program.SetUniform($"{name}.linear", Linear);
        program.SetUniform($"{name}.quadratic", Quadratic);
        program.SetUniform($"{name}.ambient", Ambient);
        program.SetUniform($"{name}.diffuse", Diffuse);
        program.SetUniform($"{name}.specular", Specular);
    }

    public readonly void Enable(string name, Program program, int index)
    {
        program.SetUniform($"{name}[{index}].position", Position);
        program.SetUniform($"{name}[{index}].direction", Direction);
        program.SetUniform($"{name}[{index}].cutOff", CutOff);
        program.SetUniform($"{name}[{index}].outerCutOff", OuterCutOff);
        program.SetUniform($"{name}[{index}].constant", Constant);
        program.SetUniform($"{name}[{index}].linear", Linear);
        program.SetUniform($"{name}[{index}].quadratic", Quadratic);
        program.SetUniform($"{name}[{index}].ambient", Ambient);
        program.SetUniform($"{name}[{index}].diffuse", Diffuse);
        program.SetUniform($"{name}[{index}].specular", Specular);
    }
}
