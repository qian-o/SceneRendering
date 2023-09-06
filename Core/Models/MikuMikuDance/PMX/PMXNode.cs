using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.PMX;

public class PMXNode : MMDNode
{
    public int DeformDepth { get; set; }

    public bool IsDeformAfterPhysics { get; set; }

    public PMXNode? AppendNode { get; set; }

    public bool IsAppendTranslate { get; set; }

    public bool IsAppendRotate { get; set; }

    public bool IsAppendLocal { get; set; }

    public float AppendWeight { get; set; }

    public Vector3D<float> AppendTranslate { get; private set; }

    public Quaternion<float> AppendRotate { get; private set; }

    public MMDIkSolver? IkSolver { get; set; }

    public PMXNode()
    {
        DeformDepth = -1;
        IsDeformAfterPhysics = false;
        AppendNode = null;
        IsAppendTranslate = false;
        IsAppendRotate = false;
        IsAppendLocal = false;
        AppendWeight = 0.0f;
        AppendTranslate = Vector3D<float>.Zero;
        AppendRotate = Quaternion<float>.Identity;
        IkSolver = null;
    }

    public void UpdateAppendTransform()
    {
        if (AppendNode == null)
        {
            return;
        }

        if (IsAppendTranslate)
        {
            Vector3D<float> appendTranslate;
            if (IsAppendLocal)
            {
                appendTranslate = AppendNode.Translate - AppendNode.InitialTranslate;
            }
            else
            {
                if (AppendNode.AppendNode != null)
                {
                    appendTranslate = AppendNode.AppendTranslate;
                }
                else
                {
                    appendTranslate = AppendNode.Translate - AppendNode.InitialTranslate;
                }
            }

            AppendTranslate = appendTranslate * AppendWeight;
        }

        if (IsAppendRotate)
        {
            Quaternion<float> appendRotate;
            if (IsAppendLocal)
            {
                appendRotate = AppendNode.AnimateRotate;
            }
            else
            {
                if (AppendNode.AppendNode != null)
                {
                    appendRotate = AppendNode.AppendRotate;
                }
                else
                {
                    appendRotate = AppendNode.AnimateRotate;
                }
            }

            if (AppendNode.EnableIK)
            {
                appendRotate *= AppendNode.IKRotate;
            }

            AppendRotate = Quaternion<float>.Slerp(Quaternion<float>.Identity, appendRotate, AppendWeight);
        }

        UpdateLocalTransform();
    }

    protected override void OnBeginUpdateTransform()
    {
        AppendTranslate = Vector3D<float>.Zero;
        AppendRotate = Quaternion<float>.Identity;
    }

    protected override void OnEndUpdateTransform()
    {

    }

    protected override void OnUpdateLocalTransform()
    {
        Vector3D<float> t = AnimateTranslate;
        if (IsAppendTranslate)
        {
            t += AppendTranslate;
        }

        Quaternion<float> r = AnimateRotate;
        if (EnableIK)
        {
            r *= IKRotate;
        }
        if (IsAppendRotate)
        {
            r = AppendRotate * r;
        }

        Vector3D<float> s = Scale;

        LocalTransform = Matrix4X4.CreateScale(s) * Matrix4X4.CreateFromQuaternion(r) * Matrix4X4.CreateTranslation(t);
    }
}