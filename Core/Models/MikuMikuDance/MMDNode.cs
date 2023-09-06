using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public abstract class MMDNode
{
    public uint Index { get; set; }

    public string Name { get; set; }

    public bool EnableIK { get; set; }

    public Vector3D<float> Translate { get; set; }

    public Quaternion<float> Rotate { get; set; }

    public Vector3D<float> Scale { get; set; }

    public Vector3D<float> AnimationTranslate { get; set; }

    public Quaternion<float> AnimationRotate { get; set; }

    public Vector3D<float> AnimateTranslate => Translate + AnimationTranslate;

    public Quaternion<float> AnimateRotate => Rotate * AnimationRotate;

    public Quaternion<float> IKRotate { get; set; }

    public MMDNode? Parent { get; protected set; }

    public MMDNode? Child { get; protected set; }

    public MMDNode? Next { get; protected set; }

    public MMDNode? Prev { get; protected set; }

    public Matrix4X4<float> LocalTransform { get; set; }

    public Matrix4X4<float> GlobalTransform { get; set; }

    public Matrix4X4<float> InverseInitTransform { get; set; }

    public Vector3D<float> InitialTranslate { get; protected set; }

    public Quaternion<float> InitialRotate { get; protected set; }

    public Vector3D<float> InitialScale { get; protected set; }

    public Vector3D<float> BaseAnimationTranslate { get; protected set; }

    public Quaternion<float> BaseAnimationRotate { get; protected set; }

    protected MMDNode()
    {
        Index = 0;
        Name = string.Empty;
        EnableIK = false;
        Parent = null;
        Child = null;
        Next = null;
        Prev = null;
        Translate = new Vector3D<float>(0.0f);
        Rotate = Quaternion<float>.Identity;
        Scale = new Vector3D<float>(1.0f);
        AnimationTranslate = new Vector3D<float>(0.0f);
        AnimationRotate = Quaternion<float>.Identity;
        BaseAnimationTranslate = new Vector3D<float>(0.0f);
        BaseAnimationRotate = Quaternion<float>.Identity;
        IKRotate = Quaternion<float>.Identity;
        LocalTransform = Matrix4X4<float>.Identity;
        GlobalTransform = Matrix4X4<float>.Identity;
        InverseInitTransform = Matrix4X4<float>.Identity;
        InitialTranslate = new Vector3D<float>(0.0f);
        InitialRotate = Quaternion<float>.Identity;
        InitialScale = new Vector3D<float>(1.0f);
    }

    public void AddChild(MMDNode value)
    {
        value.Parent = this;
        if (Child == null)
        {
            Child = value;
            Child.Next = null;
            Child.Prev = Child;
        }
        else
        {
            MMDNode? lastNode = Child.Prev;
            lastNode!.Next = value;
            value.Prev = lastNode;

            Child.Prev = value;
        }
    }

    public void BeginUpdateTransform()
    {
        LoadInitialTRS();
        IKRotate = Quaternion<float>.Identity;
        OnBeginUpdateTransform();
    }

    public void EndUpdateTransform()
    {
        OnEndUpdateTransform();
    }

    public void UpdateLocalTransform()
    {
        OnUpdateLocalTransform();
    }

    public void UpdateGlobalTransform()
    {
        if (Parent == null)
        {
            GlobalTransform = LocalTransform;
        }
        else
        {
            GlobalTransform = LocalTransform * Parent.GlobalTransform;
        }

        MMDNode? node = Child;
        while (node != null)
        {
            node.UpdateGlobalTransform();
            node = node.Next;
        }
    }

    public void UpdateChildTransform()
    {
        MMDNode? node = Child;
        while (node != null)
        {
            node.UpdateGlobalTransform();
            node = node.Next;
        }
    }

    public void CalculateInverseInitTransform()
    {
        InverseInitTransform = GlobalTransform.Invert();
    }

    public void SaveInitialTRS()
    {
        InitialTranslate = Translate;
        InitialRotate = Rotate;
        InitialScale = Scale;
    }

    public void LoadInitialTRS()
    {
        Translate = InitialTranslate;
        Rotate = InitialRotate;
        Scale = InitialScale;
    }

    public void SaveBaseAnimation()
    {
        BaseAnimationTranslate = AnimationTranslate;
        BaseAnimationRotate = AnimationRotate;
    }

    public void LoadBaseAnimation()
    {
        AnimationTranslate = BaseAnimationTranslate;
        AnimationRotate = BaseAnimationRotate;
    }

    public void ClearBaseAnimation()
    {
        BaseAnimationTranslate = new Vector3D<float>(0.0f);
        BaseAnimationRotate = Quaternion<float>.Identity;
    }

    protected virtual void OnBeginUpdateTransform() { }

    protected virtual void OnEndUpdateTransform() { }

    protected virtual void OnUpdateLocalTransform()
    {
        Matrix4X4<float> s = Matrix4X4.CreateScale(Scale);
        Matrix4X4<float> r = Matrix4X4.CreateFromQuaternion(AnimateRotate);
        Matrix4X4<float> t = Matrix4X4.CreateTranslation(AnimateTranslate);
        if (EnableIK)
        {
            r *= Matrix4X4.CreateFromQuaternion(IKRotate);
        }
        LocalTransform = s * r * t;
    }
}
