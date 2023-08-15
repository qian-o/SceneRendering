using Core.Tools;

namespace Core.Contracts.Models.ShaderStructures;

public interface IStructure
{
    void Enable(string name, Program program);

    void Enable(string name, Program program, int index);
}
