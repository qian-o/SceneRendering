using Core.Contracts.Models.ShaderStructures;
using Core.Tools;

namespace Core.Models.ShaderStructures;

public struct Material : IStructure
{
    public int Diffuse;

    public int Specular;

    public float Shininess;

    public readonly void Enable(string name, Program program)
    {
        program.SetUniform($"{name}.diffuse", Diffuse);
        program.SetUniform($"{name}.specular", Specular);
        program.SetUniform($"{name}.shininess", Shininess);
    }

    public readonly void Enable(string name, Program program, int index)
    {
        program.SetUniform($"{name}[{index}].diffuse", Diffuse);
        program.SetUniform($"{name}[{index}].specular", Specular);
        program.SetUniform($"{name}[{index}].shininess", Shininess);
    }
}
