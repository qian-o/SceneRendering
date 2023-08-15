using Core.Contracts.Models.ShaderStructures;
using Core.Tools;
using Silk.NET.Maths;

namespace Core.Models.ShaderStructures;

public struct DirLight : IStructure
{
    public Vector3D<float> Direction;

    public Vector3D<float> Ambient;

    public Vector3D<float> Diffuse;

    public Vector3D<float> Specular;

    public readonly void Enable(string name, Program program)
    {
        program.SetUniform($"{name}.direction", Direction);
        program.SetUniform($"{name}.ambient", Ambient);
        program.SetUniform($"{name}.diffuse", Diffuse);
        program.SetUniform($"{name}.specular", Specular);
    }

    public readonly void Enable(string name, Program program, int index)
    {
        program.SetUniform($"{name}[{index}].direction", Direction);
        program.SetUniform($"{name}[{index}].ambient", Ambient);
        program.SetUniform($"{name}[{index}].diffuse", Diffuse);
        program.SetUniform($"{name}[{index}].specular", Specular);
    }
}
