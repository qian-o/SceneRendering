using BulletSharp;

namespace Core.Models.MikuMikuDance;

public class MMDFilterCallback : OverlapFilterCallback
{
    public List<BroadphaseProxy> NonFilterProxy { get; } = new();

    public override bool NeedBroadphaseCollision(BroadphaseProxy proxy0, BroadphaseProxy proxy1)
    {
        if (proxy0 == null || proxy1 == null)
        {
            return false;
        }

        int findIt = NonFilterProxy.FindIndex(item => (proxy0 != null && item.UniqueId == proxy0.UniqueId) || (proxy1 != null && item.UniqueId == proxy1.UniqueId));
        if (findIt != -1)
        {
            return true;
        }

        bool collides = (proxy0.CollisionFilterGroup & proxy1.CollisionFilterMask) != 0;
        collides = collides && (proxy1.CollisionFilterGroup & proxy0.CollisionFilterMask) != 0;
        return collides;
    }
}
