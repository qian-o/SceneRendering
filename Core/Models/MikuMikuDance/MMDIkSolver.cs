using Core.Contracts.Models.MikuMikuDance;
using Core.Helpers;
using Silk.NET.Maths;
using System.Numerics;

namespace Core.Models.MikuMikuDance;

public class MMDIkSolver
{
    #region Enums
    public enum SolveAxis
    {
        X,
        Y,
        Z
    }
    #endregion

    #region Classes
    private class IKChain
    {
        public MMDNode Node { get; }

        public bool EnableAxisLimit { get; set; }

        public Vector3D<float> LowerLimit { get; set; }

        public Vector3D<float> UpperLimit { get; set; }

        public Vector3D<float> PrevAngle { get; set; }

        public Quaternion<float> SaveIKRot { get; set; }

        public float PlaneModeAngle { get; set; }

        public IKChain(MMDNode node)
        {
            Node = node;
        }
    }
    #endregion

    private readonly List<IKChain> chains = new();
    private MMDNode? ikNode;
    private MMDNode? ikTarget;
    private uint iterateCount;
    private float limitAngle;
    private bool enable;
    private bool baseAnimEnable;

    public MMDNode? IKNode { get => ikNode; set => ikNode = value; }

    public MMDNode? TargetNode { get => ikTarget; set => ikTarget = value; }

    public string Name
    {
        get
        {
            if (ikNode is null)
            {
                return string.Empty;
            }

            return ikNode.Name;
        }
    }

    public uint IterateCount { set => iterateCount = value; }

    public float LimitAngle { set => limitAngle = value; }

    public bool Enable { get => enable; set => enable = value; }

    public bool BaseAnimationEnabled => baseAnimEnable;

    public MMDIkSolver()
    {
        ikNode = null;
        ikTarget = null;
        iterateCount = 1;
        limitAngle = MathHelper.Pi * 2.0f;
        enable = true;
        baseAnimEnable = true;
    }

    public void AddIKChain(MMDNode node, bool isKnee = false)
    {
        Vector3D<float> lowerLimit = default, upperLimit = default;
        if (isKnee)
        {
            lowerLimit = new Vector3D<float>(MathHelper.DegreesToRadians(0.5f), 0.0f, 0.0f);
            upperLimit = new Vector3D<float>(MathHelper.DegreesToRadians(180.0f), 0.0f, 0.0f);
        }

        AddIKChain(node, isKnee, lowerLimit, upperLimit);
    }

    public void AddIKChain(MMDNode node, bool axisLimit, Vector3D<float> lowerLimit, Vector3D<float> upperLimit)
    {
        IKChain chain = new(node)
        {
            EnableAxisLimit = axisLimit,
            LowerLimit = lowerLimit,
            UpperLimit = upperLimit,
            SaveIKRot = Quaternion<float>.Identity
        };

        chains.Add(chain);
    }

    public void Solve()
    {
        if (!enable)
        {
            return;
        }

        if (ikNode is null || ikTarget is null)
        {
            // wrong ik
            return;
        }

        // Initialize IKChain
        foreach (IKChain chain in chains)
        {
            chain.PrevAngle = new Vector3D<float>(0.0f);
            chain.Node.IKRotate = Quaternion<float>.Identity;
            chain.PlaneModeAngle = 0.0f;

            chain.Node.UpdateLocalTransform();
            chain.Node.UpdateGlobalTransform();
        }

        float maxDist = float.MaxValue;
        for (uint i = 0; i < iterateCount; i++)
        {
            SolveCore(i);

            Vector3D<float> targetPos = ikTarget.GlobalTransform[3].ToVector3D();
            Vector3D<float> ikPos = ikNode.GlobalTransform[3].ToVector3D();
            float dist = Vector3D.Distance(targetPos, ikPos);
            if (dist < maxDist)
            {
                maxDist = dist;
                foreach (IKChain chain in chains)
                {
                    chain.SaveIKRot = chain.Node.IKRotate;
                }
            }
            else
            {
                foreach (IKChain chain in chains)
                {
                    chain.Node.IKRotate = chain.SaveIKRot;
                    chain.Node.UpdateLocalTransform();
                    chain.Node.UpdateGlobalTransform();
                }
                break;
            }
        }
    }

    public void SaveBaseAnimation()
    {
        baseAnimEnable = enable;
    }

    public void LoadBaseAnimation()
    {
        enable = baseAnimEnable;
    }

    public void ClearBaseAnimation()
    {
        baseAnimEnable = true;
    }

    private void SolveCore(uint iteration)
    {
        Vector3D<float> ikPos = ikNode!.GlobalTransform[3].ToVector3D();

        for (int chainIdx = 0; chainIdx < chains.Count; chainIdx++)
        {
            IKChain chain = chains[chainIdx];
            MMDNode chainNode = chain.Node;
            if (chainNode == ikTarget)
            {
                /*
				ターゲットとチェインが同じ場合、 chainTargetVec が0ベクトルとなる。
				その後の計算で求める回転値がnanになるため、計算を行わない
				対象モデル：ぽんぷ長式比叡.pmx
				*/
                continue;
            }

            if (chain.EnableAxisLimit)
            {
                // X,Y,Z 軸のいずれかしか回転しないものは専用の Solver を使用する
                if ((chain.LowerLimit.X != 0 || chain.UpperLimit.X != 0)
                    && (chain.LowerLimit.Y == 0 || chain.UpperLimit.Y == 0)
                    && (chain.LowerLimit.Z == 0 || chain.UpperLimit.Z == 0))
                {
                    SolvePlane(iteration, chainIdx, SolveAxis.X);
                    continue;
                }
                else if ((chain.LowerLimit.Y != 0 || chain.UpperLimit.Y != 0)
                         && (chain.LowerLimit.X == 0 || chain.UpperLimit.X == 0)
                         && (chain.LowerLimit.Z == 0 || chain.UpperLimit.Z == 0))
                {
                    SolvePlane(iteration, chainIdx, SolveAxis.Y);
                    continue;
                }
                else if ((chain.LowerLimit.Z != 0 || chain.UpperLimit.Z != 0)
                         && (chain.LowerLimit.X == 0 || chain.UpperLimit.X == 0)
                         && (chain.LowerLimit.Y == 0 || chain.UpperLimit.Y == 0))
                {
                    SolvePlane(iteration, chainIdx, SolveAxis.Z);
                    continue;
                }
            }

            Vector3D<float> targetPos = ikTarget!.GlobalTransform[3].ToVector3D();

            Matrix4X4<float> invChain = chain.Node.GlobalTransform.Invert();

            Vector3D<float> chainIkPos = (new Vector4D<float>(ikPos, 1.0f) * invChain).ToVector3D();
            Vector3D<float> chainTargetPos = (new Vector4D<float>(targetPos, 1.0f) * invChain).ToVector3D();

            Vector3D<float> chainIkVec = Vector3D.Normalize(chainIkPos);
            Vector3D<float> chainTargetVec = Vector3D.Normalize(chainTargetPos);

            float dot = Vector3D.Dot(chainTargetVec, chainIkVec);
            dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

            float angle = MathF.Acos(dot);
            float angleDeg = MathHelper.RadiansToDegrees(angle);
            if (angleDeg < 1.0e-3f)
            {
                continue;
            }
            angle = MathHelper.Clamp(angle, -limitAngle, limitAngle);
            Vector3D<float> cross = Vector3D.Normalize(Vector3D.Cross(chainTargetVec, chainIkVec));
            Quaternion<float> rot = Quaternion<float>.CreateFromAxisAngle(cross, angle);

            Quaternion<float> chainRot = rot * chainNode.AnimateRotate * chainNode.IKRotate;
            if (chain.EnableAxisLimit)
            {
                Matrix3X3<float> chainRotM = Matrix3X3.CreateFromQuaternion(chainRot);
                Vector3D<float> rotXYZ = chainRotM.GetRotation().ToVector3D();

                Vector3D<float> clampXYZ;
                clampXYZ = Vector3D.Clamp(rotXYZ, chain.LowerLimit, chain.UpperLimit);

                clampXYZ = Vector3D.Clamp(clampXYZ - chain.PrevAngle, new Vector3D<float>(-limitAngle), new Vector3D<float>(limitAngle)) + chain.PrevAngle;
                Quaternion<float> r = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(1.0f, 0.0f, 0.0f), clampXYZ.X);
                r = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0.0f, 1.0f, 0.0f), clampXYZ.Y) * r;
                r = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0.0f, 0.0f, 1.0f), clampXYZ.Z) * r;
                chainRotM = Matrix3X3.CreateFromQuaternion(r);
                chain.PrevAngle = clampXYZ;

                chainRot = Quaternion<float>.CreateFromRotationMatrix(chainRotM);
            }

            Quaternion<float> ikRot = Quaternion<float>.Inverse(chainNode.AnimateRotate) * chainRot;
            chainNode.IKRotate = ikRot;

            chainNode.UpdateLocalTransform();
            chainNode.UpdateGlobalTransform();
        }
    }

    private void SolvePlane(uint iteration, int chainIdx, SolveAxis solveAxis)
    {
        int rotateAxisIndex = 0;
        Vector3D<float> rotateAxis = new(1.0f, 0.0f, 0.0f);
        switch (solveAxis)
        {
            case SolveAxis.X:
                rotateAxisIndex = 0;
                rotateAxis = new Vector3D<float>(1.0f, 0.0f, 0.0f);
                break;
            case SolveAxis.Y:
                rotateAxisIndex = 1;
                rotateAxis = new Vector3D<float>(0.0f, 1.0f, 0.0f);
                break;
            case SolveAxis.Z:
                rotateAxisIndex = 2;
                rotateAxis = new Vector3D<float>(0.0f, 0.0f, 1.0f);
                break;
            default:
                break;
        }

        IKChain chain = chains[chainIdx];
        Vector3D<float> ikPos = ikNode!.GlobalTransform[3].ToVector3D();

        Vector3D<float> targetPos = ikTarget!.GlobalTransform[3].ToVector3D();

        Matrix4X4<float> invChain = chain.Node.GlobalTransform.Invert();

        Vector3D<float> chainIkPos = (new Vector4D<float>(ikPos, 1.0f) * invChain).ToVector3D();
        Vector3D<float> chainTargetPos = (new Vector4D<float>(targetPos, 1.0f) * invChain).ToVector3D();

        Vector3D<float> chainIkVec = Vector3D.Normalize(chainIkPos);
        Vector3D<float> chainTargetVec = Vector3D.Normalize(chainTargetPos);

        float dot = Vector3D.Dot(chainTargetVec, chainIkVec);
        dot = MathHelper.Clamp(dot, -1.0f, 1.0f);

        float angle = MathF.Acos(dot);

        angle = MathHelper.Clamp(angle, -limitAngle, limitAngle);

        Quaternion<float> rot1 = Quaternion<float>.CreateFromAxisAngle(rotateAxis, angle);
        Vector3D<float> targetVec1 = Vector3D.Transform(chainTargetVec, rot1);
        float dot1 = Vector3D.Dot(targetVec1, chainIkVec);

        Quaternion<float> rot2 = Quaternion<float>.CreateFromAxisAngle(rotateAxis, -angle);
        Vector3D<float> targetVec2 = Vector3D.Transform(chainTargetVec, rot2);
        float dot2 = Vector3D.Dot(targetVec2, chainIkVec);

        float newAngle = chain.PlaneModeAngle;
        if (dot1 > dot2)
        {
            newAngle += angle;
        }
        else
        {
            newAngle -= angle;
        }
        if (iteration == 0.0f)
        {
            if (newAngle < chain.LowerLimit[rotateAxisIndex] || newAngle > chain.UpperLimit[rotateAxisIndex])
            {
                if (-newAngle > chain.LowerLimit[rotateAxisIndex] && -newAngle < chain.UpperLimit[rotateAxisIndex])
                {
                    newAngle *= -1.0f;
                }
                else
                {
                    float halfRad = (chain.LowerLimit[rotateAxisIndex] + chain.UpperLimit[rotateAxisIndex]) * 0.5f;
                    if (MathHelper.Abs(halfRad - newAngle) > MathHelper.Abs(halfRad + newAngle))
                    {
                        newAngle *= -1.0f;
                    }
                }
            }
        }

        newAngle = MathHelper.Clamp(newAngle, chain.LowerLimit[rotateAxisIndex], chain.UpperLimit[rotateAxisIndex]);
        chain.PlaneModeAngle = newAngle;

        Quaternion<float> rot = Quaternion<float>.CreateFromAxisAngle(rotateAxis, newAngle);
        chain.Node.IKRotate = Quaternion<float>.Inverse(chain.Node.AnimateRotate) * rot;

        chain.Node.UpdateLocalTransform();
        chain.Node.UpdateGlobalTransform();
    }
}
