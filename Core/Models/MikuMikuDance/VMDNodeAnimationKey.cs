using Core.Helpers;
using Silk.NET.Maths;
using VMDMotion = Core.Models.MikuMikuDance.VMD.Motion;

namespace Core.Models.MikuMikuDance;

public struct VMDNodeAnimationKey
{
    public int Time;

    public Vector3D<float> Translate;

    public Quaternion<float> Rotate;

    public VMDBezier TxBezier;

    public VMDBezier TyBezier;

    public VMDBezier TzBezier;

    public VMDBezier RotBezier;

    public void Set(VMDMotion motion)
    {
        Time = (int)motion.Frame;

        Translate = new Vector3D<float>(1.0f, 1.0f, -1.0f) * motion.Translate;

        Quaternion<float> q = motion.Quaternion;
        Matrix3X3<float> rot0 = Matrix3X3.CreateFromQuaternion(q);
        Matrix3X3<float> rot1 = rot0.InvZ();
        Rotate = Quaternion<float>.CreateFromRotationMatrix(rot1);

        TxBezier.SetVMDBezier(motion.Interpolation[0..]);
        TyBezier.SetVMDBezier(motion.Interpolation[1..]);
        TzBezier.SetVMDBezier(motion.Interpolation[2..]);
        RotBezier.SetVMDBezier(motion.Interpolation[3..]);
    }
}
