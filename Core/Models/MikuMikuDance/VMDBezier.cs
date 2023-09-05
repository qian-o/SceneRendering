using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public unsafe struct VMDBezier
{
    public Vector2D<float> Cp1;

    public Vector2D<float> Cp2;

    public void SetVMDBezier(byte[] cp)
    {
        int x0 = cp[0];
        int y0 = cp[4];
        int x1 = cp[8];
        int y1 = cp[12];

        Cp1 = new Vector2D<float>(x0 / 127.0f, y0 / 127.0f);
        Cp2 = new Vector2D<float>(x1 / 127.0f, y1 / 127.0f);
    }

    public readonly float EvalX(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = 1.0f - t;
        float it2 = it * it;
        float it3 = it2 * it;
        float[] x = new float[] { 0.0f, Cp1.X, Cp2.X, 1 };

        return t3 * x[3] + 3 * t2 * it * x[2] + 3 * t * it2 * x[1] + it3 * x[0];
    }

    public readonly float EvalY(float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float it = 1.0f - t;
        float it2 = it * it;
        float it3 = it2 * it;
        float[] y = new float[] { 0.0f, Cp1.Y, Cp2.Y, 1 };

        return t3 * y[3] + 3 * t2 * it * y[2] + 3 * t * it2 * y[1] + it3 * y[0];
    }

    public readonly Vector2D<float> Eval(float t)
    {
        return new Vector2D<float>(EvalX(t), EvalY(t));
    }

    public readonly float FindBezierX(float time)
    {
        float e = 0.00001f;
        float start = 0.0f;
        float stop = 1.0f;
        float t = 0.5f;
        float x = EvalX(t);
        while (MathF.Abs(time - x) > e)
        {
            if (time < x)
            {
                stop = t;
            }
            else
            {
                start = t;
            }
            t = (stop + start) * 0.5f;
            x = EvalX(t);
        }

        return t;
    }
}
