namespace Core.Models.MikuMikuDance;

public abstract class MMDMorphManager
{
    public abstract int GetMorphCount();

    public abstract int FindMorphIndex(string name);

    public abstract MMDMorph GetMorph(int index);

    public MMDMorph? GetMorph(string name)
    {
        int findIndex = FindMorphIndex(name);

        if (findIndex == -1)
        {
            return null;
        }

        return GetMorph(findIndex);
    }
}
