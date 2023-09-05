using BulletSharp;
using Core.Helpers;
using Evergine.Mathematics;
using PMXJoint = Core.Models.MikuMikuDance.PMX.Joint;

namespace Core.Models.MikuMikuDance;

public class MMDJoint
{
    private TypedConstraint? constraint;

    public TypedConstraint? Constraint => constraint;

    public MMDJoint()
    {
    }

    public bool CreateJoint(PMXJoint pmxJoint, MMDRigidBody rigidBodyA, MMDRigidBody rigidBodyB)
    {
        Destroy();

        Matrix4x4 transform = Matrix4x4.CreateFromYawPitchRoll(pmxJoint.Rotate.Z, pmxJoint.Rotate.Y, pmxJoint.Rotate.X) * Matrix4x4.CreateTranslation(pmxJoint.Translate.ToBulletVector3());

        rigidBodyA.RigidBody!.GetWorldTransform(out Matrix4x4 invA);
        invA.Invert();
        rigidBodyB.RigidBody!.GetWorldTransform(out Matrix4x4 invB);
        invB.Invert();

        invA = transform * invA;
        invB = transform * invB;

        Generic6DofSpringConstraint springConstraint = new(rigidBodyA.RigidBody,
                                                           rigidBodyB.RigidBody,
                                                           invA,
                                                           invB,
                                                           true)
        {
            LinearLowerLimit = pmxJoint.TranslateLimitMin.ToBulletVector3(),
            LinearUpperLimit = pmxJoint.TranslateLimitMax.ToBulletVector3(),
            AngularLowerLimit = pmxJoint.RotateLimitMin.ToBulletVector3(),
            AngularUpperLimit = pmxJoint.RotateLimitMax.ToBulletVector3()
        };

        if (pmxJoint.SpringTranslate.X != 0.0f)
        {
            springConstraint.EnableSpring(0, true);
            springConstraint.SetStiffness(0, pmxJoint.SpringTranslate.X);
        }
        if (pmxJoint.SpringTranslate.Y != 0.0f)
        {
            springConstraint.EnableSpring(1, true);
            springConstraint.SetStiffness(1, pmxJoint.SpringTranslate.Y);
        }
        if (pmxJoint.SpringTranslate.Z != 0.0f)
        {
            springConstraint.EnableSpring(2, true);
            springConstraint.SetStiffness(2, pmxJoint.SpringTranslate.Z);
        }
        if (pmxJoint.SpringRotate.X != 0.0f)
        {
            springConstraint.EnableSpring(3, true);
            springConstraint.SetStiffness(3, pmxJoint.SpringRotate.X);
        }
        if (pmxJoint.SpringRotate.Y != 0.0f)
        {
            springConstraint.EnableSpring(4, true);
            springConstraint.SetStiffness(4, pmxJoint.SpringRotate.Y);
        }
        if (pmxJoint.SpringRotate.Z != 0.0f)
        {
            springConstraint.EnableSpring(5, true);
            springConstraint.SetStiffness(5, pmxJoint.SpringRotate.Z);
        }

        constraint = springConstraint;

        return true;
    }

    public void Destroy()
    {
        constraint?.Dispose();
        constraint = null;
    }
}
