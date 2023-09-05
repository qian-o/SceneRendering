using Core.Helpers;
using Evergine.Mathematics;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.MotionStates;

public class DynamicAndBoneMergeMotionState : MMDMotionState
{
    private readonly MMDNode _node;
    private readonly Matrix4X4<float> _offset;
    private readonly Matrix4X4<float> _invOffset;
    private readonly bool _override;
    private Matrix4x4 transform;

    public DynamicAndBoneMergeMotionState(MMDNode node, Matrix4X4<float> offset, bool @override = true)
    {
        _node = node;
        _offset = offset;
        _override = @override;

        _invOffset = offset.Invert();
        Reset();
    }

    public override void Reset()
    {
        transform = Matrix4X4.Transpose((_offset * _node.GlobalTransform).InvZ()).ToBulletMatrix4x4();
    }

    public override void ReflectGlobalTransform()
    {
        Matrix4X4<float> world = Matrix4X4.Transpose(transform.ToMatrix());
        Matrix4X4<float> btGlobal = _invOffset * world.InvZ();
        Matrix4X4<float> global = _node.GlobalTransform;
        btGlobal = new Matrix4X4<float>(btGlobal[0], btGlobal[1], btGlobal[2], global[3]);

        if (_override)
        {
            _node.GlobalTransform = btGlobal;
            _node.UpdateChildTransform();
        }
    }

    public override void GetWorldTransform(out Matrix4x4 worldTrans)
    {
        worldTrans = transform;
    }

    public override void SetWorldTransform(ref Matrix4x4 worldTrans)
    {
        transform = worldTrans;
    }
}
