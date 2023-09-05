namespace Core.Models.MikuMikuDance;

public class MMDPhysicsManager
{
    private MMDPhysics? mmdPhysics;

    public MMDPhysics? MMDPhysics => mmdPhysics;

    public List<MMDRigidBody> RigidBodys { get; } = new List<MMDRigidBody>();

    public List<MMDJoint> Joints { get; } = new List<MMDJoint>();

    public MMDPhysicsManager()
    {

    }

    ~MMDPhysicsManager()
    {
        foreach (MMDRigidBody rigidBody in RigidBodys)
        {
            mmdPhysics?.RemoveRigidBody(rigidBody);
        }
        RigidBodys.Clear();

        foreach (MMDJoint joint in Joints)
        {
            mmdPhysics?.RemoveJoint(joint);
        }
        Joints.Clear();
    }

    public bool Create()
    {
        mmdPhysics = new MMDPhysics();
        return mmdPhysics.Create();
    }

    public MMDRigidBody AddRigidBody()
    {
        MMDRigidBody rigidBody = new();

        RigidBodys.Add(rigidBody);

        return rigidBody;
    }

    public MMDJoint AddJoint()
    {
        MMDJoint joint = new();

        Joints.Add(joint);

        return joint;
    }
}
