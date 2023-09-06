using Core.Helpers;
using KeyType = Core.Models.MikuMikuDance.VMDMorphAnimationKey;

namespace Core.Models.MikuMikuDance;

public class VMDMorphController
{
    private int startKeyIndex;

    public MMDMorph? Morph { get; set; }

    public List<KeyType> Keys { get; } = new List<KeyType>();

    public VMDMorphController()
    {
        startKeyIndex = 0;
    }

    public void Evaluate(float t, float animWeight = 1.0f)
    {
        if (Morph == null)
        {
            return;
        }

        if (Keys.Count == 0)
        {
            return;
        }

        float weight;
        int index = FindBoundIndex((int)t, startKeyIndex);
        if (index == -1)
        {
            weight = Keys[^1].Weight;
        }
        else if (index == 0)
        {
            weight = Keys[0].Weight;
        }
        else
        {
            VMDMorphAnimationKey key0 = Keys[index - 1];
            VMDMorphAnimationKey key1 = Keys[index];

            float timeRange = (float)key1.Time - key0.Time;
            float time = (t - key0.Time) / timeRange;
            weight = (key1.Weight - key0.Weight) * time + key0.Weight;

            startKeyIndex = index;
        }

        if (animWeight == 1.0f)
        {
            Morph.Weight = weight;
        }
        else
        {
            Morph.Weight = MathHelper.Lerp(Morph.BaseAnimationWeight, weight, animWeight);
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

    private int FindBoundIndex(int t, int startIndex)
    {
        if (!Keys.Any() || Keys.Count <= startIndex)
        {
            return -1;
        }

        KeyType key0 = Keys[startIndex];
        if (key0.Time <= t)
        {
            if (startIndex + 1 < Keys.Count)
            {
                KeyType key1 = Keys[startIndex + 1];
                if (key1.Time > t)
                {
                    return startIndex + 1;
                }
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (startIndex != 0)
            {
                KeyType key1 = Keys[startIndex - 1];
                if (key1.Time <= t)
                {
                    return startIndex;
                }
            }
            else
            {
                return 0;
            }
        }

        return Keys.FindIndex(0, key => key.Time > t);
    }
}
