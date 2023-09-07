using Core.Helpers;
using Core.Models.MikuMikuDance.VPD;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

#region Structs
public struct MMDSubMesh
{
    public int BeginIndex;

    public int VertexCount;

    public int MaterialID;
}
#endregion

public unsafe abstract class MMDModel
{
    #region Structs
    private struct Pose
    {
        public MMDNode? Node;

        public Vector3D<float> BeginTranslate;

        public Vector3D<float> EndTranslate;

        public Quaternion<float> BeginRotate;

        public Quaternion<float> EndRotate;
    }

    private struct Morph
    {
        public MMDMorph? MMDMorph;

        public float BeginWeight;

        public float EndWeight;
    }
    #endregion

    public abstract MMDNodeManager? GetNodeManager();

    public abstract MMDIkManager? GetIkManager();

    public abstract MMDMorphManager? GetMorphManager();

    public abstract MMDPhysicsManager? GetPhysicsManager();

    public abstract int GetVertexCount();

    public abstract Vector3D<float>* GetPositions();

    public abstract Vector3D<float>* GetNormals();

    public abstract Vector2D<float>* GetUVs();

    public abstract Vector3D<float>* GetUpdatePositions();

    public abstract Vector3D<float>* GetUpdateNormals();

    public abstract Vector2D<float>* GetUpdateUVs();

    public abstract int GetIndexElementSize();

    public abstract int GetIndexCount();

    public abstract void* GetIndices();

    public abstract int GetMaterialCount();

    public abstract MMDMaterial* GetMaterials();

    public abstract int GetSubMeshCount();

    public abstract MMDSubMesh* GetSubMeshes();

    public abstract MMDPhysics? GetMMDPhysics();

    // ノードを初期化する
    public abstract void InitializeAnimation();

    // ベースアニメーション(アニメーション読み込み時、Physics反映用)
    public void SaveBaseAnimation()
    {
        MMDNodeManager nodeMan = GetNodeManager()!;
        for (int i = 0; i < nodeMan.GetNodeCount(); i++)
        {
            MMDNode node = nodeMan.GetMMDNode(i);
            node.SaveBaseAnimation();
        }

        MMDMorphManager morphMan = GetMorphManager()!;
        for (int i = 0; i < morphMan.GetMorphCount(); i++)
        {
            MMDMorph morph = morphMan.GetMorph(i);
            morph.SaveBaseAnimation();
        }

        MMDIkManager ikMan = GetIkManager()!;
        for (int i = 0; i < ikMan.GetIkSolverCount(); i++)
        {
            MMDIkSolver ikSolver = ikMan.GetMMDIkSolver(i);
            ikSolver.SaveBaseAnimation();
        }
    }

    public void LoadBaseAnimation()
    {
        MMDNodeManager nodeMan = GetNodeManager()!;
        for (int i = 0; i < nodeMan.GetNodeCount(); i++)
        {
            MMDNode node = nodeMan.GetMMDNode(i);
            node.LoadBaseAnimation();
        }

        MMDMorphManager morphMan = GetMorphManager()!;
        for (int i = 0; i < morphMan.GetMorphCount(); i++)
        {
            MMDMorph morph = morphMan.GetMorph(i);
            morph.LoadBaseAnimation();
        }

        MMDIkManager ikMan = GetIkManager()!;
        for (int i = 0; i < ikMan.GetIkSolverCount(); i++)
        {
            MMDIkSolver ikSolver = ikMan.GetMMDIkSolver(i);
            ikSolver.LoadBaseAnimation();
        }
    }

    public void ClearBaseAnimation()
    {
        MMDNodeManager nodeMan = GetNodeManager()!;
        for (int i = 0; i < nodeMan.GetNodeCount(); i++)
        {
            MMDNode node = nodeMan.GetMMDNode(i);
            node.ClearBaseAnimation();
        }

        MMDMorphManager morphMan = GetMorphManager()!;
        for (int i = 0; i < morphMan.GetMorphCount(); i++)
        {
            MMDMorph morph = morphMan.GetMorph(i);
            morph.ClearBaseAnimation();
        }

        MMDIkManager ikMan = GetIkManager()!;
        for (int i = 0; i < ikMan.GetIkSolverCount(); i++)
        {
            MMDIkSolver ikSolver = ikMan.GetMMDIkSolver(i);
            ikSolver.ClearBaseAnimation();
        }
    }

    // アニメーションの前後で呼ぶ (VMDアニメーションの前後)
    public abstract void BeginAnimation();

    public abstract void EndAnimation();

    // Morph
    public abstract void UpdateMorphAnimation();

    // ノードを更新する
    public abstract void UpdateNodeAnimation(bool afterPhysicsAnim);

    // Physicsを更新する
    public abstract void ResetPhysics();

    public abstract void UpdatePhysicsAnimation(float elapsed);

    // 頂点を更新する
    public abstract void Update();

    public abstract void SetParallelUpdateHint(uint parallelCount);

    public void UpdateAllAnimation(VMDAnimation? vmdAnim, float vmdFrame, float physicsElapsed)
    {
        vmdAnim?.Evaluate(vmdFrame);

        UpdateMorphAnimation();

        UpdateNodeAnimation(false);

        UpdatePhysicsAnimation(physicsElapsed);

        UpdateNodeAnimation(true);
    }

    public void LoadPose(VPDFile vpd, int frameCount = 30)
    {
        List<Pose> poses = new();
        foreach (VPD.Bone bone in vpd.Bones)
        {
            int index = GetNodeManager()!.FindNodeIndex(bone.Name);
            if (index != -1)
            {
                MMDNode node = GetNodeManager()!.GetMMDNode(bone.Name)!;

                Pose pose = new()
                {
                    Node = node,
                    BeginTranslate = node.AnimationTranslate,
                    EndTranslate = new Vector3D<float>(1, 1, -1) * bone.Translate,
                    BeginRotate = node.AnimationRotate,
                    EndRotate = bone.Quaternion.InvZ()
                };

                poses.Add(pose);
            }
        }

        List<Morph> morphs = new();
        foreach (VPD.Morph vpdMorph in vpd.Morphs)
        {
            int index = GetMorphManager()!.FindMorphIndex(vpdMorph.Name);
            if (index != -1)
            {
                MMDMorph mmdMorph = GetMorphManager()!.GetMorph(index)!;

                Morph morph = new()
                {
                    MMDMorph = mmdMorph,
                    BeginWeight = mmdMorph.Weight,
                    EndWeight = vpdMorph.Weight
                };

                morphs.Add(morph);
            }
        }

        // Physicsを反映する
        for (int i = 0; i < frameCount; i++)
        {
            BeginAnimation();

            // evaluate
            float w = (1.0f + i) / frameCount;

            foreach (Pose pose in poses)
            {
                pose.Node!.AnimationTranslate = Vector3D.Lerp(pose.BeginTranslate, pose.EndTranslate, w);
                pose.Node.AnimationRotate = Quaternion<float>.Lerp(pose.BeginRotate, pose.EndRotate, w);
            }

            foreach (Morph morph in morphs)
            {
                morph.MMDMorph!.Weight = MathHelper.Lerp(morph.BeginWeight, morph.EndWeight, w);
            }

            UpdateMorphAnimation();
            UpdateNodeAnimation(false);
            UpdatePhysicsAnimation(1.0f / 30.0f);
            UpdateNodeAnimation(true);

            EndAnimation();
        }
    }

    protected class MMDNodeManagerT<TNode> : MMDNodeManager where TNode : MMDNode
    {
        public List<TNode> Nodes { get; } = new List<TNode>();

        public override int GetNodeCount()
        {
            return Nodes.Count;
        }

        public override int FindNodeIndex(string name)
        {
            return Nodes.FindIndex(node => node.Name == name);
        }

        public override MMDNode GetMMDNode(int index)
        {
            return Nodes[index];
        }

        public TNode AddNode()
        {
            TNode node = Activator.CreateInstance<TNode>();
            node.Index = (uint)Nodes.Count;
            Nodes.Add(node);

            return node;
        }
    }

    protected class MMDIkManagerT<TIkSolver> : MMDIkManager where TIkSolver : MMDIkSolver
    {
        public List<TIkSolver> IkSolvers { get; } = new List<TIkSolver>();

        public override int GetIkSolverCount()
        {
            return IkSolvers.Count;
        }

        public override int FindIkSolverIndex(string name)
        {
            return IkSolvers.FindIndex(ik => ik.Name == name);
        }

        public override MMDIkSolver GetMMDIkSolver(int index)
        {
            return IkSolvers[index];
        }

        public TIkSolver AddIkSolver()
        {
            TIkSolver ik = Activator.CreateInstance<TIkSolver>();
            IkSolvers.Add(ik);

            return ik;
        }
    }

    protected class MMDMorphManagerT<TMorph> : MMDMorphManager where TMorph : MMDMorph
    {
        public List<TMorph> Morphs { get; } = new List<TMorph>();

        public override int GetMorphCount()
        {
            return Morphs.Count;
        }

        public override int FindMorphIndex(string name)
        {
            return Morphs.FindIndex(morph => morph.Name == name);
        }

        public override MMDMorph GetMorph(int index)
        {
            return Morphs[index];
        }

        public TMorph AddMorph()
        {
            TMorph morph = Activator.CreateInstance<TMorph>();
            Morphs.Add(morph);

            return morph;
        }
    }
}
