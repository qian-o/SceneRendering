using BulletSharp;
using Evergine.Mathematics;

namespace Core.Models.MikuMikuDance;

public class MMDPhysics
{
    private BroadphaseInterface? broadphase;
    private DefaultCollisionConfiguration? collisionConfig;
    private CollisionDispatcher? dispatcher;
    private SequentialImpulseConstraintSolver? solver;
    private DiscreteDynamicsWorld? world;
    private CollisionShape? groundShape;
    private MotionState? groundMS;
    private RigidBody? groundRB;
    private float fps;
    private int maxSubStepCount;

    public float FPS { get => fps; set => fps = value; }

    public int MaxSubStepCount { get => maxSubStepCount; set => maxSubStepCount = value; }

    public DiscreteDynamicsWorld? DynamicsWorld => world;

    public MMDPhysics()
    {
        fps = 120.0f;
        maxSubStepCount = 10;
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

        world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig)
        {
            Gravity = new Vector3(0.0f, -9.8f * 10.0f, 0.0f)
        };

        groundShape = new StaticPlaneShape(new Vector3(0.0f, 1.0f, 0.0f), 0.0f);

        groundMS = new DefaultMotionState(Matrix4x4.Identity);

        RigidBodyConstructionInfo groundInfo = new(0.0f, groundMS, groundShape, new Vector3(0.0f, 0.0f, 0.0f));
        groundRB = new RigidBody(groundInfo);

        world.AddRigidBody(groundRB);

        MMDFilterCallback callback = new();
        callback.NonFilterProxy.Add(groundRB.BroadphaseProxy);
        world.PairCache.SetOverlapFilterCallback(callback);

        return true;
    }

    public void Destroy()
    {
        if (groundRB != null)
        {
            world?.RemoveRigidBody(groundRB);
        }

        broadphase?.Dispose();
        collisionConfig?.Dispose();
        dispatcher?.Dispose();
        solver?.Dispose();
        world?.Dispose();
        groundShape?.Dispose();
        groundMS?.Dispose();
        groundRB?.Dispose();
    }

    public void Update(float time)
    {
        world?.StepSimulation(time, MaxSubStepCount, 1.0f / fps);
    }

    public void AddRigidBody(MMDRigidBody mmdRB)
    {
        world?.AddRigidBody(mmdRB.RigidBody, 1 << mmdRB.Group, mmdRB.GroupMask);
    }

    public void RemoveRigidBody(MMDRigidBody mmdRB)
    {
        world?.RemoveRigidBody(mmdRB.RigidBody);
    }

    public void AddJoint(MMDJoint mmdJoint)
    {
        if (mmdJoint.Constraint != null)
        {
            world?.AddConstraint(mmdJoint.Constraint);
        }
    }

    public void RemoveJoint(MMDJoint mmdJoint)
    {
        if (mmdJoint.Constraint != null)
        {
            world?.RemoveConstraint(mmdJoint.Constraint);
        }
    }
}