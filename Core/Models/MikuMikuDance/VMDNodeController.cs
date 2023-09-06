﻿using Core.Helpers;
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

        int index = FindBoundIndex((int)t, startKeyIndex);

        Vector3D<float> vt;
        Quaternion<float> q;
        if (index == -1)
        {
            vt = Keys[^1].Translate;
            q = Keys[^1].Rotate;
        }
        else if (index == 0)
        {
            vt = Keys[0].Translate;
            q = Keys[0].Rotate;
        }
        else
        {
            KeyType key0 = Keys[index - 1];
            KeyType key1 = Keys[index];

            float timeRange = (float)key1.Time - key0.Time;
            float time = (t - key0.Time) / timeRange;
            float tx_x = key0.TxBezier.FindBezierX(time);
            float ty_x = key0.TyBezier.FindBezierX(time);
            float tz_x = key0.TzBezier.FindBezierX(time);
            float rot_x = key0.RotBezier.FindBezierX(time);
            float tx_y = key0.TxBezier.EvalY(tx_x);
            float ty_y = key0.TyBezier.EvalY(ty_x);
            float tz_y = key0.TzBezier.EvalY(tz_x);
            float rot_y = key0.RotBezier.EvalY(rot_x);

            vt = GLM.Mix(key0.Translate, key1.Translate, new Vector3D<float>(tx_y, ty_y, tz_y));
            q = Quaternion<float>.Slerp(key0.Rotate, key1.Rotate, rot_y);

            startKeyIndex = index;
        }

        if (weight == 1.0f)
        {
            Node.AnimationRotate = q;
            Node.AnimationTranslate = vt;
        }
        else
        {
            Quaternion<float> baseQ = Node.BaseAnimationRotate;
            Vector3D<float> baseT = Node.BaseAnimationTranslate;
            Node.AnimationRotate = Quaternion<float>.Slerp(baseQ, q, weight);
            Node.AnimationTranslate = Vector3D.Lerp(baseT, vt, weight);
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