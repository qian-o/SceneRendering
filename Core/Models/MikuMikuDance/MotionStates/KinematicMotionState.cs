using Core.Helpers;
using Evergine.Mathematics;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.MotionStates;

public class KinematicMotionState : MMDMotionState
{
    private readonly MMDNode? _node;
    private readonly Matrix4X4<float> _offset;

    public KinematicMotionState(MMDNode? node, Matrix4X4<float> offset)
    {
        _node = node;
        _offset = offset;
    }

    public override void Reset()
    {

    }

    public override void ReflectGlobalTransform()
    {

    }

    public override void GetWorldTransform(out Matrix4x4 worldTrans)
    {
        Matrix4X4<float> m;
        if (_node != null)
        {
            m = _offset * _node.GlobalTransform;
        }
        else
        {
            m = _offset;
        }
        m = m.InvZ();
        worldTrans = Matrix4X4.Transpose(m).ToBulletMatrix4x4();
    }

    public override void SetWorldTransform(ref Matrix4x4 worldTrans)
    {

    }
}
