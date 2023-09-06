using BulletSharp;
using Evergine.Mathematics;

namespace Core.Models.MikuMikuDance;

public class MMDPhysics
{
    private BroadphaseInterface? broadphase;
    private DefaultCollisionConfiguration? collisionConfig;
    private CollisionDispatcher? dispatcher;
    private SequentialImpulseConstraintSolver? solver;
    private CollisionShape? groundShape;
    private MotionState? groundMS;
    private RigidBody? groundRB;

    public float FPS { get; set; }

    public int MaxSubStepCount { get; set; }

    public DiscreteDynamicsWorld? DynamicsWorld { get; private set; }

    public MMDPhysics()
    {
        FPS = 120.0f;
        MaxSubStepCount = 10;
    }

    ~MMDPhysics()
    {
        Destroy();
    }

    public bool Create()
    {
        broadphase = new DbvtBroadphase();
        collisionConfig = new DefaultCollisionConfiguration();
        dispatcher = new CollisionDispatcher(collisionConfig);

        solver = new SequentialImpulseConstraintSolver();

        DynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig)
        {
            Gravity = new Vector3(0.0f, -9.8f * 10.0f, 0.0f)
        };

        groundShape = new StaticPlaneShape(new Vector3(0.0f, 1.0f, 0.0f), 0.0f);

        groundMS = new DefaultMotionState(Matrix4x4.Identity);

        RigidBodyConstructionInfo groundInfo = new(0.0f, groundMS, groundShape, new Vector3(0.0f, 0.0f, 0.0f));
        groundRB = new RigidBody(groundInfo);

        DynamicsWorld.AddRigidBody(groundRB);

        MMDFilterCallback callback = new();
        callback.NonFilterProxy.Add(groundRB.BroadphaseProxy);
        DynamicsWorld.PairCache.SetOverlapFilterCallback(callback);

        return true;
    }

    public void Destroy()
    {
        if (groundRB != null)
        {
            DynamicsWorld?.RemoveRigidBody(groundRB);
        }

        broadphase?.Dispose();
        collisionConfig?.Dispose();
        dispatcher?.Dispose();
        solver?.Dispose();
        DynamicsWorld?.Dispose();
        groundShape?.Dispose();
        groundMS?.Dispose();
        groundRB?.Dispose();
    }

    public void Update(float time)
    {
        DynamicsWorld?.StepSimulation(time, MaxSubStepCount, 1.0f / FPS);
    }

    public void AddRigidBody(MMDRigidBody mmdRB)
    {
        DynamicsWorld?.AddRigidBody(mmdRB.RigidBody, 1 << mmdRB.Group, mmdRB.GroupMask);
    }

    public void RemoveRigidBody(MMDRigidBody mmdRB)
    {
        DynamicsWorld?.RemoveRigidBody(mmdRB.RigidBody);
    }

    public void AddJoint(MMDJoint mmdJoint)
    {
        if (mmdJoint.Constraint != null)
        {
            DynamicsWorld?.AddConstraint(mmdJoint.Constraint);
        }
    }

    public void RemoveJoint(MMDJoint mmdJoint)
    {
        if (mmdJoint.Constraint != null)
        {
            DynamicsWorld?.RemoveConstraint(mmdJoint.Constraint);
        }
    }
}