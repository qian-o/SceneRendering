using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.PMX;

#region Enums
public enum SkinningType
{
    Weight1,
    Weight2,
    Weight4,
    SDEF,
    DualQuaternion
}
#endregion

#region Structs
public unsafe struct SDEF
{
    public fixed int BoneIndex[2];

    public float BoneWeight;

    public Vector3D<float> C;

    public Vector3D<float> R0;

    public Vector3D<float> R1;
}

public unsafe struct VertexBoneInfo
{
    public SkinningType SkinningType;

    public fixed int BoneIndex[4];

    public fixed float BoneWeight[4];

    public SDEF SDEF;
}
#endregion

public class PMXModel : MMDModel
{
    #region Enums
    public enum MorphType
    {
        None,
        Position,
        UV,
        Material,
        Bone,
        Group
    }
    #endregion

    #region Structs
    private struct PositionMorph
    {
        public uint Index;

        public Vector3D<float> Position;
    }

    private struct PositionMorphData
    {
        public PositionMorph[] MorphVertices;
    }

    private struct UVMorph
    {
        public uint Index;

        public Vector4D<float> UV;
    }

    private struct UVMorphData
    {
        public UVMorph[] MorphUVs;
    }

    private struct MaterialFactor
    {
        public Vector3D<float> Diffuse;

        public float Alpha;

        public Vector3D<float> Specular;

        public float SpecularPower;

        public Vector3D<float> Ambient;

        public Vector4D<float> EdgeColor;

        public float EdgeSize;

        public Vector4D<float> TextureFactor;

        public Vector4D<float> SphereTextureFactor;

        public Vector4D<float> ToonTextureFactor;

        public MaterialFactor(MaterialMorph pmxMat) : this()
        {
            Diffuse.X = pmxMat.Diffuse.X;
            Diffuse.Y = pmxMat.Diffuse.Y;
            Diffuse.Z = pmxMat.Diffuse.Z;
            Alpha = pmxMat.Diffuse.W;
            Specular = pmxMat.Specular;
            SpecularPower = pmxMat.SpecularPower;
            Ambient = pmxMat.Ambient;
            EdgeColor = pmxMat.EdgeColor;
            EdgeSize = pmxMat.EdgeSize;
            TextureFactor = pmxMat.TextureCoefficient;
            SphereTextureFactor = pmxMat.SphereTextureCoefficient;
            ToonTextureFactor = pmxMat.ToonTextureCoefficient;
        }

        public void Mul(MaterialFactor val, float weight)
        {
            Diffuse = Vector3D.Lerp(Diffuse, val.Diffuse, weight);
            Alpha = MathHelper.Lerp(Alpha, val.Alpha, weight);
            Specular = Vector3D.Lerp(Specular, val.Specular, weight);
            SpecularPower = MathHelper.Lerp(SpecularPower, val.SpecularPower, weight);
            Ambient = Vector3D.Lerp(Ambient, val.Ambient, weight);
            EdgeColor = Vector4D.Lerp(EdgeColor, val.EdgeColor, weight);
            EdgeSize = MathHelper.Lerp(EdgeSize, val.EdgeSize, weight);
            TextureFactor = Vector4D.Lerp(TextureFactor, val.TextureFactor, weight);
            SphereTextureFactor = Vector4D.Lerp(SphereTextureFactor, val.SphereTextureFactor, weight);
            ToonTextureFactor = Vector4D.Lerp(ToonTextureFactor, val.ToonTextureFactor, weight);
        }

        public void Add(MaterialFactor val, float weight)
        {
            Diffuse += val.Diffuse * weight;
            Alpha += val.Alpha * weight;
            Specular += val.Specular * weight;
            SpecularPower += val.SpecularPower * weight;
            Ambient += val.Ambient * weight;
            EdgeColor += val.EdgeColor * weight;
            EdgeSize += val.EdgeSize * weight;
            TextureFactor += val.TextureFactor * weight;
            SphereTextureFactor += val.SphereTextureFactor * weight;
            ToonTextureFactor += val.ToonTextureFactor * weight;
        }
    }

    private struct MaterialMorphData
    {
        public MaterialMorph[] MaterialMorphs;
    }

    private struct BoneMorphElement
    {
        public MMDNode? Node;

        public Vector3D<float> Position;

        public Quaternion<float> Rotate;
    }

    private struct BoneMorphData
    {
        public BoneMorphElement[] BoneMorphs;
    }

    private struct GroupMorphData
    {
        public GroupMorph[] GroupMorphs;
    }

    private struct UpdateRange
    {
        public int VertexOffset;

        public int VertexCount;
    }
    #endregion

    #region Classes
    private class PMXMorph : MMDMorph
    {
        public MorphType MorphType;

        public int DataIndex;
    }
    #endregion

    #region Fields
    private Vector3D<float>[] positions = Array.Empty<Vector3D<float>>();
    private Vector3D<float>[] normals = Array.Empty<Vector3D<float>>();
    private Vector2D<float>[] uvs = Array.Empty<Vector2D<float>>();
    private VertexBoneInfo[] vertexBoneInfos = Array.Empty<VertexBoneInfo>();
    private Vector3D<float>[] updatePositions = Array.Empty<Vector3D<float>>();
    private Vector3D<float>[] updateNormals = Array.Empty<Vector3D<float>>();
    private Vector2D<float>[] updateUVs = Array.Empty<Vector2D<float>>();
    private Matrix4X4<float>[] transforms = Array.Empty<Matrix4X4<float>>();

    private char[] indices = Array.Empty<char>();
    private int indexCount;
    private int indexElementSize;

    private PositionMorphData[] positionMorphDatas = Array.Empty<PositionMorphData>();
    private UVMorphData[] uvMorphDatas = Array.Empty<UVMorphData>();
    private MaterialMorphData[] materialMorphDatas = Array.Empty<MaterialMorphData>();
    private BoneMorphData[] boneMorphDatas = Array.Empty<BoneMorphData>();
    private GroupMorphData[] groupMorphDatas = Array.Empty<GroupMorphData>();

    // PositionMorph用
    private Vector3D<float>[] morphPositions = Array.Empty<Vector3D<float>>();
    private Vector4D<float>[] morphUVs = Array.Empty<Vector4D<float>>();

    // マテリアルMorph用
    private MMDMaterial[] initMaterials = Array.Empty<MMDMaterial>();
    private MaterialFactor[] mulMaterialFactors = Array.Empty<MaterialFactor>();
    private MaterialFactor[] addMaterialFactors = Array.Empty<MaterialFactor>();

    private Vector3D<float> bboxMin;
    private Vector3D<float> bboxMax;

    private MMDMaterial[] materials = Array.Empty<MMDMaterial>();
    private MMDSubMesh[] subMeshes = Array.Empty<MMDSubMesh>();
    private PMXNode[] sortedNodes = Array.Empty<PMXNode>();

    private MMDNodeManagerT<PMXNode>? nodeMan;
    private MMDIkManagerT<MMDIkSolver>? ikSolverMan;
    private MMDMorphManagerT<PMXMorph>? morphMan;
    private MMDPhysicsManager? physicsMan;

    private int parallelUpdateCount;
    private UpdateRange[] updateRanges = Array.Empty<UpdateRange>();
    #endregion

    #region Properties
    public Vector3D<float> BBoxMin => bboxMin;

    public Vector3D<float> BBoxMax => bboxMax;
    #endregion

    public PMXModel()
    {

    }

    ~PMXModel()
    {
        Destroy();
    }

    public bool Load(string path, string mmdDataDir)
    {
        Destroy();

        PMXFile pmx = new(path);

        string dirPath = Path.GetDirectoryName(path)!;

        return true;
    }

    public void Destroy()
    {
        materials = Array.Empty<MMDMaterial>();
        subMeshes = Array.Empty<MMDSubMesh>();

        positions = Array.Empty<Vector3D<float>>();
        normals = Array.Empty<Vector3D<float>>();
        uvs = Array.Empty<Vector2D<float>>();
        vertexBoneInfos = Array.Empty<VertexBoneInfo>();

        indices = Array.Empty<char>();

        nodeMan?.Nodes.Clear();

        updateRanges = Array.Empty<UpdateRange>();
    }

    private void SetupParallelUpdate()
    {
        if (parallelUpdateCount == 0)
        {
            parallelUpdateCount = Environment.ProcessorCount;
        }

        int maxParallelCount = MathHelper.Max(16, Environment.ProcessorCount);
        if (parallelUpdateCount > maxParallelCount)
        {
            parallelUpdateCount = 16;
        }

        Array.Resize(ref updateRanges, parallelUpdateCount);

        int vertexCount = positions.Length;
        int lowerVertexCount = 1000;
        if (vertexCount < updateRanges.Length * lowerVertexCount)
        {
            int numRanges = (vertexCount + lowerVertexCount - 1) / lowerVertexCount;
            for (int rangeIdx = 0; rangeIdx < updateRanges.Length; rangeIdx++)
            {
                UpdateRange range = new();
                if (rangeIdx < numRanges)
                {
                    range.VertexOffset = rangeIdx * lowerVertexCount;
                    range.VertexCount = MathHelper.Min(lowerVertexCount, vertexCount - range.VertexOffset);
                }
                else
                {
                    range.VertexOffset = 0;
                    range.VertexCount = 0;
                }

                updateRanges[rangeIdx] = range;
            }
        }
        else
        {
            int numVertexCount = vertexCount / updateRanges.Length;
            int offset = 0;
            for (int rangeIdx = 0; rangeIdx < updateRanges.Length; rangeIdx++)
            {
                UpdateRange range = new()
                {
                    VertexOffset = offset,
                    VertexCount = numVertexCount
                };

                if (rangeIdx == 0)
                {
                    range.VertexCount += vertexCount % updateRanges.Length;
                }

                offset = range.VertexOffset + range.VertexCount;

                updateRanges[rangeIdx] = range;
            }
        }
    }

    private void Update(UpdateRange range)
    {

    }

    private void Morph(PMXMorph morph, float weight)
    {
        switch (morph.MorphType)
        {
            case MorphType.Position:
                MorphPosition(positionMorphDatas[morph.DataIndex], weight);
                break;
            case MorphType.UV:
                MorphUV(uvMorphDatas[morph.DataIndex], weight);
                break;
            case MorphType.Material:
                MorphMaterial(materialMorphDatas[morph.DataIndex], weight);
                break;
            case MorphType.Bone:
                MorphBone(boneMorphDatas[morph.DataIndex], weight);
                break;
            case MorphType.Group:
                {
                    GroupMorphData groupMorphData = groupMorphDatas[morph.DataIndex];
                    foreach (GroupMorph groupMorph in groupMorphData.GroupMorphs)
                    {
                        if (groupMorph.MorphIndex == -1)
                        {
                            continue;
                        }

                        PMXMorph elemMorph = morphMan!.Morphs[groupMorph.MorphIndex];
                        Morph(elemMorph, groupMorph.Weight * weight);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void MorphPosition(PositionMorphData morphData, float weight)
    {
        if (weight == 0)
        {
            return;
        }

        foreach (PositionMorph morphVtx in morphData.MorphVertices)
        {
            morphPositions[morphVtx.Index] += morphVtx.Position * weight;
        }
    }

    private void MorphUV(UVMorphData morphData, float weight)
    {
        if (weight == 0)
        {
            return;
        }

        foreach (UVMorph morphVtx in morphData.MorphUVs)
        {
            morphUVs[morphVtx.Index] += morphVtx.UV * weight;
        }
    }

    private void BeginMorphMaterial()
    {
        MaterialFactor initMul = new()
        {
            Diffuse = Vector3D<float>.One,
            Alpha = 1.0f,
            Specular = Vector3D<float>.One,
            SpecularPower = 1.0f,
            Ambient = Vector3D<float>.One,
            EdgeColor = Vector4D<float>.One,
            EdgeSize = 1.0f,
            TextureFactor = Vector4D<float>.One,
            SphereTextureFactor = Vector4D<float>.One,
            ToonTextureFactor = Vector4D<float>.One
        };

        MaterialFactor initAdd = new()
        {
            Diffuse = Vector3D<float>.Zero,
            Alpha = 0.0f,
            Specular = Vector3D<float>.Zero,
            SpecularPower = 0.0f,
            Ambient = Vector3D<float>.Zero,
            EdgeColor = Vector4D<float>.Zero,
            EdgeSize = 0.0f,
            TextureFactor = Vector4D<float>.Zero,
            SphereTextureFactor = Vector4D<float>.Zero,
            ToonTextureFactor = Vector4D<float>.Zero
        };

        int matCount = materials.Length;
        for (int matIdx = 0; matIdx < matCount; matIdx++)
        {
            mulMaterialFactors[matIdx] = initMul;
            mulMaterialFactors[matIdx].Diffuse = initMaterials[matIdx].Diffuse;
            mulMaterialFactors[matIdx].Alpha = initMaterials[matIdx].Alpha;
            mulMaterialFactors[matIdx].Specular = initMaterials[matIdx].Specular;
            mulMaterialFactors[matIdx].SpecularPower = initMaterials[matIdx].SpecularPower;
            mulMaterialFactors[matIdx].Ambient = initMaterials[matIdx].Ambient;

            addMaterialFactors[matIdx] = initAdd;
        }
    }

    private void EndMorphMaterial()
    {
        int matCount = materials.Length;
        for (int matIdx = 0; matIdx < matCount; matIdx++)
        {
            MaterialFactor matFactor = mulMaterialFactors[matIdx];
            matFactor.Add(addMaterialFactors[matIdx], 1.0f);

            materials[matIdx].Diffuse = matFactor.Diffuse;
            materials[matIdx].Alpha = matFactor.Alpha;
            materials[matIdx].Specular = matFactor.Specular;
            materials[matIdx].SpecularPower = matFactor.SpecularPower;
            materials[matIdx].Ambient = matFactor.Ambient;
            materials[matIdx].TextureMulFactor = mulMaterialFactors[matIdx].TextureFactor;
            materials[matIdx].TextureAddFactor = addMaterialFactors[matIdx].TextureFactor;
            materials[matIdx].SpTextureMulFactor = mulMaterialFactors[matIdx].SphereTextureFactor;
            materials[matIdx].SpTextureAddFactor = addMaterialFactors[matIdx].SphereTextureFactor;
            materials[matIdx].ToonTextureMulFactor = mulMaterialFactors[matIdx].ToonTextureFactor;
            materials[matIdx].ToonTextureAddFactor = addMaterialFactors[matIdx].ToonTextureFactor;
        }
    }

    private void MorphMaterial(MaterialMorphData morphData, float weight)
    {
        foreach (MaterialMorph matMorph in morphData.MaterialMorphs)
        {
            if (matMorph.MaterialIndex != -1)
            {
                int mi = matMorph.MaterialIndex;
                switch (matMorph.OpType)
                {
                    case OpType.Mul:
                        mulMaterialFactors[mi].Mul(new MaterialFactor(matMorph), weight);
                        break;
                    case OpType.Add:
                        mulMaterialFactors[mi].Add(new MaterialFactor(matMorph), weight);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (matMorph.OpType)
                {
                    case OpType.Mul:
                        for (int i = 0; i < materials.Length; i++)
                        {
                            mulMaterialFactors[i].Mul(new MaterialFactor(matMorph), weight);
                        }
                        break;
                    case OpType.Add:
                        for (int i = 0; i < materials.Length; i++)
                        {
                            addMaterialFactors[i].Add(new MaterialFactor(matMorph), weight);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void MorphBone(BoneMorphData morphData, float weight)
    {
        foreach (BoneMorphElement boneMorph in morphData.BoneMorphs)
        {
            MMDNode node = boneMorph.Node!;
            Vector3D<float> t = Vector3D.Lerp(Vector3D<float>.Zero, boneMorph.Position, weight);
            node.Translate += t;
            Quaternion<float> q = Quaternion<float>.Slerp(node.Rotate, boneMorph.Rotate, weight);
            node.Rotate = q;
        }
    }

    public override MMDNodeManager? GetNodeManager()
    {
        return nodeMan;
    }

    public override MMDIkManager? GetIkManager()
    {
        return ikSolverMan;
    }

    public override MMDMorphManager? GetMorphManager()
    {
        return morphMan;
    }

    public override MMDPhysicsManager? GetPhysicsManager()
    {
        return physicsMan;
    }

    public override int GetVertexCount()
    {
        return positions.Length;
    }

    public override unsafe Vector3D<float>* GetPositions()
    {
        return positions.Data();
    }

    public override unsafe Vector3D<float>* GetNormals()
    {
        return normals.Data();
    }

    public override unsafe Vector2D<float>* GetUVs()
    {
        return uvs.Data();
    }

    public override unsafe Vector3D<float>* GetUpdatePositions()
    {
        return updatePositions.Data();
    }

    public override unsafe Vector3D<float>* GetUpdateNormals()
    {
        return updateNormals.Data();
    }

    public override unsafe Vector2D<float>* GetUpdateUVs()
    {
        return updateUVs.Data();
    }

    public override int GetIndexCount()
    {
        return indexCount;
    }

    public override int GetIndexElementSize()
    {
        return indexElementSize;
    }

    public override unsafe void* GetIndices()
    {
        return indices.Data();
    }

    public override int GetMaterialCount()
    {
        return materials.Length;
    }

    public override unsafe MMDMaterial* GetMaterials()
    {
        return materials.Data();
    }

    public override int GetSubMeshCount()
    {
        return subMeshes.Length;
    }

    public override unsafe MMDSubMesh* GetSubMeshes()
    {
        return subMeshes.Data();
    }

    public override MMDPhysics? GetMMDPhysics()
    {
        return physicsMan?.MMDPhysics;
    }

    public override void InitializeAnimation()
    {
        ClearBaseAnimation();

        foreach (PMXNode node in nodeMan!.Nodes)
        {
            node.AnimationTranslate = Vector3D<float>.Zero;
            node.AnimationRotate = Quaternion<float>.Identity;
        }

        BeginAnimation();

        foreach (PMXNode node in nodeMan.Nodes)
        {
            node.UpdateLocalTransform();
        }

        foreach (PMXMorph morph in morphMan!.Morphs)
        {
            morph.Weight = 0.0f;
        }

        foreach (MMDIkSolver ikSolver in ikSolverMan!.IkSolvers)
        {
            ikSolver.Enable = true;
        }

        foreach (PMXNode node in nodeMan.Nodes)
        {
            if (node.Parent == null)
            {
                node.UpdateGlobalTransform();
            }
        }

        foreach (PMXNode pmxNode in sortedNodes)
        {
            if (pmxNode.AppendNode != null)
            {
                pmxNode.UpdateAppendTransform();
                pmxNode.UpdateGlobalTransform();
            }

            if (pmxNode.IkSolver != null)
            {
                pmxNode.IkSolver.Solve();
                pmxNode.UpdateGlobalTransform();
            }
        }

        foreach (PMXNode node in nodeMan.Nodes)
        {
            if (node.Parent == null)
            {
                node.UpdateGlobalTransform();
            }
        }

        EndAnimation();

        ResetPhysics();
    }

    public override void BeginAnimation()
    {
        foreach (PMXNode node in nodeMan!.Nodes)
        {
            node.BeginUpdateTransform();
        }

        int vtxCount = morphPositions.Length;
        for (int i = 0; i < vtxCount; i++)
        {
            morphPositions[i] = Vector3D<float>.Zero;
            morphUVs[i] = Vector4D<float>.Zero;
        }
    }

    public override void EndAnimation()
    {
        foreach (PMXNode node in nodeMan!.Nodes)
        {
            node.EndUpdateTransform();
        }
    }

    public override void UpdateMorphAnimation()
    {
        // Morph の処理
        BeginMorphMaterial();

        List<PMXMorph> morphs = morphMan!.Morphs;
        for (int i = 0; i < morphs.Count; i++)
        {
            Morph(morphs[i], morphs[i].Weight);
        }

        EndMorphMaterial();
    }

    public override void UpdateNodeAnimation(bool afterPhysicsAnim)
    {
        foreach (PMXNode pmxNode in sortedNodes)
        {
            if (pmxNode.IsDeformAfterPhysics != afterPhysicsAnim)
            {
                continue;
            }

            pmxNode.UpdateLocalTransform();
        }

        foreach (PMXNode pmxNode in sortedNodes)
        {
            if (pmxNode.IsDeformAfterPhysics != afterPhysicsAnim)
            {
                continue;
            }

            if (pmxNode.Parent == null)
            {
                pmxNode.UpdateGlobalTransform();
            }
        }

        foreach (PMXNode pmxNode in sortedNodes)
        {
            if (pmxNode.IsDeformAfterPhysics != afterPhysicsAnim)
            {
                continue;
            }

            if (pmxNode.AppendNode != null)
            {
                pmxNode.UpdateAppendTransform();
                pmxNode.UpdateGlobalTransform();
            }

            if (pmxNode.IkSolver != null)
            {
                pmxNode.IkSolver.Solve();
                pmxNode.UpdateGlobalTransform();
            }
        }

        foreach (PMXNode pmxNode in sortedNodes)
        {
            if (pmxNode.IsDeformAfterPhysics != afterPhysicsAnim)
            {
                continue;
            }

            if (pmxNode.Parent == null)
            {
                pmxNode.UpdateGlobalTransform();
            }
        }
    }

    public override void ResetPhysics()
    {
        MMDPhysicsManager physicsMan = GetPhysicsManager()!;
        MMDPhysics? physics = physicsMan.MMDPhysics;

        if (physics == null)
        {
            return;
        }

        List<MMDRigidBody> rigidbodys = physicsMan.RigidBodys;
        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.SetActivation(false);
            rb.ResetTransform();
        }

        physics.Update(1.0f / 60.0f);

        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.ReflectGlobalTransform();
        }

        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.CalcLocalTransform();
        }

        foreach (PMXNode node in nodeMan!.Nodes)
        {
            if (node.Parent == null)
            {
                node.UpdateGlobalTransform();
            }
        }

        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.Reset(physics);
        }
    }

    public override void UpdatePhysicsAnimation(float elapsed)
    {
        MMDPhysicsManager physicsMan = GetPhysicsManager()!;
        MMDPhysics? physics = physicsMan.MMDPhysics;

        if (physics == null)
        {
            return;
        }

        List<MMDRigidBody> rigidbodys = physicsMan.RigidBodys;
        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.SetActivation(true);
        }

        physics.Update(elapsed);

        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.ReflectGlobalTransform();
        }

        foreach (MMDRigidBody rb in rigidbodys)
        {
            rb.CalcLocalTransform();
        }

        foreach (PMXNode node in nodeMan!.Nodes)
        {
            if (node.Parent == null)
            {
                node.UpdateGlobalTransform();
            }
        }
    }

    public override void Update()
    {
        List<PMXNode> nodes = nodeMan!.Nodes;

        // スキンメッシュに使用する変形マトリクスを事前計算
        for (int i = 0; i < nodes.Count; i++)
        {
            transforms[i] = nodes[i].InverseInitTransform * nodes[i].GlobalTransform;
        }

        SetupParallelUpdate();

        Update(updateRanges[0]);

        Parallel.For(0, parallelUpdateCount - 1, (i) =>
        {
            Update(updateRanges[i + 1]);
        });
    }
}