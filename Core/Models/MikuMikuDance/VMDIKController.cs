using KeyType = Core.Models.MikuMikuDance.VMDIKAnimationKey;

namespace Core.Models.MikuMikuDance;

public class VMDIKController
{
    private int startKeyIndex;

    public MMDIkSolver? IkSolver { get; set; }

    public List<KeyType> Keys { get; } = new List<KeyType>();

    public VMDIKController()
    {
        startKeyIndex = 0;
    }

    public void Evaluate(float t, float weight = 1.0f)
    {
        if (IkSolver == null)
        {
            return;
        }

        if (Keys.Count == 0)
        {
            return;
        }

        int index = FindBoundIndex((int)t, startKeyIndex);
        bool enable;
        if (index == -1)
        {
            enable = Keys[^1].Enable;
        }
        else if (index == 0)
        {
            enable = Keys[0].Enable;
        }
        else
        {
            enable = Keys[index - 1].Enable;

            startKeyIndex = index;
        }

        if (weight == 1.0f)
        {
            IkSolver.Enable = enable;
        }
        else
        {
            if (weight < 1.0f)
            {
                IkSolver.Enable = IkSolver.BaseAnimationEnabled;
            }
            else
            {
                IkSolver.Enable = enable;
            }
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
