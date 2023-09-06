using BulletSharp;
using Core.Helpers;
using Core.Models.MikuMikuDance.MotionStates;
using Evergine.Mathematics;
using Silk.NET.Maths;
using DefaultMotionState = Core.Models.MikuMikuDance.MotionStates.DefaultMotionState;
using MathHelper = Core.Helpers.MathHelper;
using PMXRigidbody = Core.Models.MikuMikuDance.PMX.Rigidbody;

namespace Core.Models.MikuMikuDance;

public class MMDRigidBody
{
    #region Enums
    private enum RigidBodyType
    {
        Kinematic,
        Dynamic,
        Aligned
    }
    #endregion

    private CollisionShape? shape;
    private MMDMotionState? activeMotionState;
    private MMDMotionState? kinematicMotionState;
    private RigidBodyType rigidBodyType;
    private MMDNode? node;
    private Matrix4X4<float> offsetMat;

    public RigidBody? RigidBody { get; private set; }

    public ushort Group { get; private set; }

    public ushort GroupMask { get; private set; }

    public string Name { get; private set; }

    public MMDRigidBody()
    {
        shape = null;
        activeMotionState = null;
        kinematicMotionState = null;
        RigidBody = null;
        rigidBodyType = RigidBodyType.Kinematic;
        Group = 0;
        GroupMask = 0;
        node = null;
        offsetMat = Matrix4X4<float>.Identity;
        Name = string.Empty;
    }

    public bool Create(PMXRigidbody pmxRigidBody, MMDModel model, MMDNode node)
    {
        Destroy();

        switch (pmxRigidBody.Shape)
        {
            case PMX.Shape.Sphere:
                shape = new SphereShape(pmxRigidBody.ShapeSize.X);
                break;
            case PMX.Shape.Box:
                shape = new BoxShape(pmxRigidBody.ShapeSize.X,
                                     pmxRigidBody.ShapeSize.Y,
                                     pmxRigidBody.ShapeSize.Z);
                break;
            case PMX.Shape.Capsule:
                shape = new CapsuleShape(pmxRigidBody.ShapeSize.X,
                                         pmxRigidBody.ShapeSize.Y);
                break;
            default:
                break;
        }

        if (shape == null)
        {
            return false;
        }

        float mass = 0.0f;
        Vector3 localInteria = new();
        if (pmxRigidBody.Op != PMX.Operation.Static)
        {
            mass = pmxRigidBody.Mass;
        }
        if (mass != 0.0f)
        {
            shape.CalculateLocalInertia(mass, out localInteria);
        }

        Matrix4X4<float> rx = Matrix4X4.CreateRotationX(pmxRigidBody.Rotate.X, new Vector3D<float>(1.0f, 0.0f, 0.0f));
        Matrix4X4<float> ry = Matrix4X4.CreateRotationY(pmxRigidBody.Rotate.Y, new Vector3D<float>(0.0f, 1.0f, 0.0f));
        Matrix4X4<float> rz = Matrix4X4.CreateRotationZ(pmxRigidBody.Rotate.Z, new Vector3D<float>(0.0f, 0.0f, 1.0f));
        Matrix4X4<float> rotMat = rz * rx * ry;
        Matrix4X4<float> translateMat = Matrix4X4.CreateTranslation(pmxRigidBody.Translate);

        Matrix4X4<float> rbMat = (rotMat * translateMat).InvZ();

        MMDNode kinematicNode;
        if (node != null)
        {
            offsetMat = rbMat * node.GlobalTransform.Invert();
            kinematicNode = node;
        }
        else
        {
            MMDNode root = model.GetNodeManager()!.GetMMDNode(0);
            offsetMat = rbMat * root.GlobalTransform.Invert();
            kinematicNode = root;
        }

        MotionState? motionState = null;
        if (pmxRigidBody.Op == PMX.Operation.Static)
        {
            kinematicMotionState = new KinematicMotionState(kinematicNode, offsetMat);
            motionState = kinematicMotionState;
        }
        else
        {
            if (node != null)
            {
                if (pmxRigidBody.Op == PMX.Operation.Dynamic)
                {
                    activeMotionState = new DynamicMotionState(kinematicNode, offsetMat);
                    kinematicMotionState = new KinematicMotionState(kinematicNode, offsetMat);
                    motionState = activeMotionState;
                }
                else if (pmxRigidBody.Op == PMX.Operation.DynamicAndBoneMerge)
                {
                    activeMotionState = new DynamicAndBoneMergeMotionState(kinematicNode, offsetMat);
                    kinematicMotionState = new KinematicMotionState(kinematicNode, offsetMat);
                    motionState = activeMotionState;
                }
            }
            else
            {
                activeMotionState = new DefaultMotionState(offsetMat);
                kinematicMotionState = new KinematicMotionState(kinematicNode, offsetMat);
                motionState = activeMotionState;
            }
        }

        RigidBodyConstructionInfo rbInfo = new(mass, motionState, shape, localInteria)
        {
            LinearDamping = pmxRigidBody.TranslateDimmer,
            AngularDamping = pmxRigidBody.RotateDimmer,
            Restitution = pmxRigidBody.Repulsion,
            Friction = pmxRigidBody.Friction,
            AdditionalDamping = true
        };

        RigidBody = new RigidBody(rbInfo)
        {
            UserObject = this
        };
        RigidBody.SetSleepingThresholds(0.01f, MathHelper.DegreesToRadians(0.1f));
        RigidBody.ActivationState = ActivationState.DisableDeactivation;
        if (pmxRigidBody.Op == PMX.Operation.Static)
        {
            RigidBody.CollisionFlags |= CollisionFlags.KinematicObject;
        }

        rigidBodyType = pmxRigidBody.Op switch
        {
            PMX.Operation.Static => RigidBodyType.Kinematic,
            PMX.Operation.Dynamic => RigidBodyType.Dynamic,
            PMX.Operation.DynamicAndBoneMerge => RigidBodyType.Dynamic,
            _ => RigidBodyType.Kinematic,
        };
        Group = pmxRigidBody.Group;
        GroupMask = pmxRigidBody.CollisionGroup;
        this.node = node;
        Name = pmxRigidBody.Name;

        return true;
    }

    public void Destroy()
    {
        shape?.Dispose();
        shape = null;
    }

    public void SetActivation(bool activation)
    {
        if (rigidBodyType != RigidBodyType.Kinematic)
        {
            if (activation)
            {
                RigidBody!.CollisionFlags &= ~CollisionFlags.KinematicObject;
                RigidBody.MotionState = activeMotionState;
            }
            else
            {
                RigidBody!.CollisionFlags |= CollisionFlags.KinematicObject;
                RigidBody.MotionState = kinematicMotionState;
            }
        }
        else
        {
            RigidBody!.MotionState = kinematicMotionState;
        }
    }

    public void ResetTransform()
    {
        activeMotionState?.Reset();
    }

    public void Reset(MMDPhysics physics)
    {
        physics.DynamicsWorld?.PairCache.CleanProxyFromPairs(RigidBody!.BroadphaseHandle, physics.DynamicsWorld.Dispatcher);

        RigidBody!.AngularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        RigidBody.LinearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        RigidBody.ClearForces();
    }

    public void ReflectGlobalTransform()
    {
        activeMotionState?.ReflectGlobalTransform();

        kinematicMotionState?.ReflectGlobalTransform();
    }

    public void CalcLocalTransform()
    {
        if (node != null)
        {
            MMDNode? parent = node.Parent;
            if (parent != null)
            {
                Matrix4X4<float> local = node.GlobalTransform * parent.GlobalTransform.Invert();
                node.LocalTransform = local;
            }
            else
            {
                node.LocalTransform = node.GlobalTransform;
            }
        }
    }

    public Matrix4X4<float> GetTransform()
    {
        return Matrix4X4.Transpose(RigidBody!.CenterOfMassTransform.ToMatrix()).InvZ();
    }
}
