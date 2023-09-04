using Silk.NET.Maths;

namespace Core.Contracts.Models.MikuMikuDance;

public unsafe abstract class MMDNode
{
    protected uint index;
    protected string name;
    protected bool enableIK;
    protected MMDNode? parent;
    protected MMDNode? child;
    protected MMDNode? next;
    protected MMDNode? prev;
    protected Vector3D<float> translate;
    protected Quaternion<float> rotate;
    protected Vector3D<float> scale;
    protected Vector3D<float> animTranslate;
    protected Quaternion<float> animRotate;
    protected Vector3D<float> baseAnimTranslate;
    protected Quaternion<float> baseAnimRotate;
    protected Quaternion<float> ikRotate;
    protected Matrix4X4<float> local;
    protected Matrix4X4<float> global;
    protected Matrix4X4<float> inverseInit;
    protected Vector3D<float> initTranslate;
    protected Quaternion<float> initRotate;
    protected Vector3D<float> initScale;

    public uint Index { get => index; set => index = value; }

    public string Name { get => name; set => name = value; }

    public bool EnableIK { get => enableIK; set => enableIK = value; }

    public Vector3D<float> Translate { get => translate; set => translate = value; }

    public Quaternion<float> Rotate { get => rotate; set => rotate = value; }

    public Vector3D<float> Scale { get => scale; set => scale = value; }

    public Vector3D<float> AnimationTranslate { get => animTranslate; set => animTranslate = value; }

    public Quaternion<float> AnimationRotate { get => animRotate; set => animRotate = value; }

    public Vector3D<float> AnimateTranslate => translate + animTranslate;

    public Quaternion<float> AnimateRotate => rotate * animRotate;

    public Quaternion<float> IKRotate { get => ikRotate; set => ikRotate = value; }

    public MMDNode? Parent => parent;

    public MMDNode? Child => child;

    public MMDNode? Next => next;

    public MMDNode? Prev => prev;

    public Matrix4X4<float> LocalTransform { get => local; set => local = value; }

    public Matrix4X4<float> GlobalTransform { get => global; set => global = value; }

    public Matrix4X4<float> InverseInitTransform { get => inverseInit; set => inverseInit = value; }

    public Vector3D<float> InitialTranslate => initTranslate;

    public Quaternion<float> InitialRotate => initRotate;

    public Vector3D<float> InitialScale => initScale;

    public Vector3D<float> BaseAnimationTranslate => baseAnimTranslate;

    public Quaternion<float> BaseAnimationRotate => baseAnimRotate;

    protected MMDNode()
    {
        index = 0;
        name = string.Empty;
        enableIK = false;
        parent = null;
        child = null;
        next = null;
        prev = null;
        translate = new Vector3D<float>(0.0f);
        rotate = Quaternion<float>.Identity;
        scale = new Vector3D<float>(1.0f);
        animTranslate = new Vector3D<float>(0.0f);
        animRotate = Quaternion<float>.Identity;
        baseAnimTranslate = new Vector3D<float>(0.0f);
        baseAnimRotate = Quaternion<float>.Identity;
        ikRotate = Quaternion<float>.Identity;
        local = Matrix4X4<float>.Identity;
        global = Matrix4X4<float>.Identity;
        inverseInit = Matrix4X4<float>.Identity;
        initTranslate = new Vector3D<float>(0.0f);
        initRotate = Quaternion<float>.Identity;
        initScale = new Vector3D<float>(1.0f);
    }

    public void AddChild(MMDNode value)
    {
        value.parent = this;
        if (child == null)
        {
            child = value;
            child.next = null;
            child.prev = child;
        }
        else
        {
            MMDNode? lastNode = child.prev;
            lastNode!.next = value;
            value.prev = lastNode;

            child.prev = value;
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
        if (parent == null)
        {
            global = local;
        }
        else
        {
            global = local * parent.global;
        }

        MMDNode? node = child;
        while (node != null)
        {
            node.UpdateGlobalTransform();
            node = node.next;
        }
    }

    public void UpdateChildTransform()
    {
        MMDNode? node = child;
        while (node != null)
        {
            node.UpdateGlobalTransform();
            node = node.next;
        }
    }

    public void CalculateInverseInitTransform()
    {
        Matrix4X4.Invert(global, out Matrix4X4<float> result);
        inverseInit = result;
    }

    public void SaveInitialTRS()
    {
        initTranslate = translate;
        initRotate = rotate;
        initScale = scale;
    }

    public void LoadInitialTRS()
    {
        translate = initTranslate;
        rotate = initRotate;
        scale = initScale;
    }

    public void SaveBaseAnimation()
    {
        baseAnimTranslate = animTranslate;
        baseAnimRotate = animRotate;
    }

    public void LoadBaseAnimation()
    {
        animTranslate = baseAnimTranslate;
        animRotate = baseAnimRotate;
    }

    public void ClearBaseAnimation()
    {
        baseAnimTranslate = new Vector3D<float>(0.0f);
        baseAnimRotate = Quaternion<float>.Identity;
    }

    protected virtual void OnBeginUpdateTransform() { }

    protected virtual void OnEndUpdateTransform() { }

    protected virtual void OnUpdateLocalTransform()
    {
        Matrix4X4<float> s = Matrix4X4.CreateScale(Scale);
        Matrix4X4<float> r = Matrix4X4.CreateFromQuaternion(AnimateRotate);
        Matrix4X4<float> t = Matrix4X4.CreateTranslation(AnimateTranslate);
        if (enableIK)
        {
            r *= Matrix4X4.CreateFromQuaternion(IKRotate);
        }
        local = s * r * t;
    }
}
