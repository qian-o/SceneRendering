using Core.Helpers;
using Evergine.Mathematics;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.MotionStates;

public class DefaultMotionState : MMDMotionState
{
    private Matrix4x4 initialTransform;
    private Matrix4x4 transform;

    public DefaultMotionState(Matrix4X4<float> matrix)
    {
        Matrix4X4<float> trans = matrix.InvZ();
        transform = Matrix4X4.Transpose(trans).ToBulletMatrix();
        initialTransform = transform;
    }

    public override void Reset()
    {
        transform = initialTransform;
    }

    public override void ReflectGlobalTransform()
    {

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
