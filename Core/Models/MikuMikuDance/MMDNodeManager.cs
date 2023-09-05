namespace Core.Models.MikuMikuDance;

public abstract class MMDNodeManager
{
    public abstract int GetNodeCount();

    public abstract int FindNodeIndex(string name);

    public abstract MMDNode GetMMDNode(int index);

    public MMDNode? GetMMDNode(string nodeName)
    {
        int findIndex = FindNodeIndex(nodeName);

        if (findIndex == -1)
        {
            return null;
        }

        return GetMMDNode(findIndex);
    }
}
