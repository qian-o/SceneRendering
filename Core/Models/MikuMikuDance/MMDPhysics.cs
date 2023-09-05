using BulletSharp;

namespace Core.Models.MikuMikuDance;

public class MMDPhysics
{
    private BroadphaseInterface broadphase;
    private DefaultCollisionConfiguration collisionConfiguration;
    private CollisionDispatcher dispatcher;
    private SequentialImpulseConstraintSolver solver;
    private DiscreteDynamicsWorld world;
    private CollisionShape collisionShape;
    private MotionState groundMS;
    private RigidBody groundRB;
    private OverlapFilterCallback filterCB;
    private double fps;
    private int maxSubStepCount;
}