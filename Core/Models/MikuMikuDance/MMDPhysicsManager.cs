namespace Core.Models.MikuMikuDance;

public class MMDPhysicsManager
{
    public MMDPhysics? MMDPhysics { get; private set; }

    public List<MMDRigidBody> RigidBodys { get; } = new List<MMDRigidBody>();

    public List<MMDJoint> Joints { get; } = new List<MMDJoint>();

    public MMDPhysicsManager()
    {

    }

    ~MMDPhysicsManager()
    {
        foreach (MMDRigidBody rigidBody in RigidBodys)
        {
            MMDPhysics?.RemoveRigidBody(rigidBody);
        }
        RigidBodys.Clear();

        foreach (MMDJoint joint in Joints)
        {
            MMDPhysics?.RemoveJoint(joint);
        }
        Joints.Clear();
    }

    public bool Create()
    {
        MMDPhysics = new MMDPhysics();
        return MMDPhysics.Create();
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
