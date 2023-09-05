using Silk.NET.Maths;
using KeyType = Core.Models.MikuMikuDance.VMDNodeAnimationKey;

namespace Core.Models.MikuMikuDance;

public class VMDNodeController
{
    private int startKeyIndex;

    public MMDNode? Node { get; set; }

    public List<KeyType> Keys { get; } = new List<KeyType>();

    public VMDNodeController()
    {
        startKeyIndex = 0;
    }

    public void Evaluate(float t, float weight = 1.0f)
    {
        if (Node == null)
        {
            return;
        }

        if (Keys.Count == 0)
        {
            Node.AnimationTranslate = new Vector3D<float>(0.0f);
            Node.AnimationRotate = new Quaternion<float>(0.0f, 0.0f, 0.0f, 1.0f);

            return;
        }
    }

    public void AddKey(KeyType key)
    {
        Keys.Add(key);
    }

    public void SortKeys()
    {
        Keys.Sort((a, b) => a.Time.CompareTo(b.Time));
    }
}
