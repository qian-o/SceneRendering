using Core.Helpers;
using Silk.NET.Maths;
using System.Runtime.InteropServices;

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

public unsafe class PMXModel : MMDModel
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

    private class PositionMorphData
    {
        public List<PositionMorph> MorphVertices { get; set; } = new();
    }

    private struct UVMorph
    {
        public uint Index;

        public Vector4D<float> UV;
    }

    private class UVMorphData
    {
        public List<UVMorph> MorphUVs { get; set; } = new();
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

    private class BoneMorphData
    {
        public List<BoneMorphElement> BoneMorphs { get; set; } = new();
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

    private readonly List<PositionMorphData> positionMorphDatas = new();
    private readonly List<UVMorphData> uvMorphDatas = new();
    private readonly List<MaterialMorphData> materialMorphDatas = new();
    private readonly List<BoneMorphData> boneMorphDatas = new();
    private readonly List<GroupMorphData> groupMorphDatas = new();

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

        int vertexCount = pmx.Vertices.Count;
        Array.Resize(ref positions, vertexCount);
        Array.Resize(ref normals, vertexCount);
        Array.Resize(ref uvs, vertexCount);
        Array.Resize(ref vertexBoneInfos, vertexCount);
        bboxMax = new Vector3D<float>(float.MinValue);
        bboxMin = new Vector3D<float>(float.MaxValue);

        bool warnSDEF = false;
        bool infoQDEF = false;
        for (int i = 0; i < pmx.Vertices.Count; i++)
        {
            Vertex v = pmx.Vertices[i];

            Vector3D<float> pos = v.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f);
            Vector3D<float> nor = v.Normal * new Vector3D<float>(1.0f, 1.0f, -1.0f);
            Vector2D<float> uv = new(v.UV.X, 1.0f - v.UV.Y);
            positions[i] = pos;
            normals[i] = nor;
            uvs[i] = uv;

            VertexBoneInfo vtxBoneInfo = new();
            if (v.WeightType == VertexWeight.SDEF)
            {
                vtxBoneInfo.BoneIndex[0] = v.BoneIndices[0];
                vtxBoneInfo.BoneIndex[1] = v.BoneIndices[1];
                vtxBoneInfo.BoneIndex[2] = v.BoneIndices[2];
                vtxBoneInfo.BoneIndex[3] = v.BoneIndices[3];

                vtxBoneInfo.BoneWeight[0] = v.BoneWeights[0];
                vtxBoneInfo.BoneWeight[1] = v.BoneWeights[1];
                vtxBoneInfo.BoneWeight[2] = v.BoneWeights[2];
                vtxBoneInfo.BoneWeight[3] = v.BoneWeights[3];
            }

            switch (v.WeightType)
            {
                case VertexWeight.BDEF1:
                    vtxBoneInfo.SkinningType = SkinningType.Weight1;
                    break;
                case VertexWeight.BDEF2:
                    vtxBoneInfo.SkinningType = SkinningType.Weight2;
                    vtxBoneInfo.BoneWeight[1] = 1.0f - vtxBoneInfo.BoneWeight[0];
                    break;
                case VertexWeight.BDEF4:
                    vtxBoneInfo.SkinningType = SkinningType.Weight4;
                    break;
                case VertexWeight.SDEF:
                    if (!warnSDEF)
                    {
                        Console.WriteLine("Use SDEF");

                        warnSDEF = true;
                    }
                    vtxBoneInfo.SkinningType = SkinningType.SDEF;
                    {
                        int i0 = vtxBoneInfo.BoneIndex[0];
                        int i1 = vtxBoneInfo.BoneIndex[1];
                        float w0 = vtxBoneInfo.BoneWeight[0];
                        float w1 = 1.0f - w0;

                        Vector3D<float> center = v.SdefC * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> r0 = v.SdefR0 * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> r1 = v.SdefR1 * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                        Vector3D<float> rw = (r0 * w0) + (r1 * w1);
                        r0 = center + r0 - rw;
                        r1 = center + r1 - rw;
                        Vector3D<float> cr0 = (center + r0) * 0.5f;
                        Vector3D<float> cr1 = (center + r1) * 0.5f;

                        vtxBoneInfo.SDEF.BoneIndex[0] = v.BoneIndices[0];
                        vtxBoneInfo.SDEF.BoneIndex[1] = v.BoneIndices[1];
                        vtxBoneInfo.SDEF.BoneWeight = v.BoneWeights[0];
                        vtxBoneInfo.SDEF.C = center;
                        vtxBoneInfo.SDEF.R0 = r0;
                        vtxBoneInfo.SDEF.R1 = r1;
                    }
                    break;
                case VertexWeight.QDEF:
                    vtxBoneInfo.SkinningType = SkinningType.DualQuaternion;
                    if (!infoQDEF)
                    {
                        Console.WriteLine("Use QDEF");

                        infoQDEF = true;
                    }
                    break;
                default:
                    vtxBoneInfo.SkinningType = SkinningType.Weight1;
                    Console.WriteLine("Unknown WeightType");
                    break;
            }
            vertexBoneInfos[i] = vtxBoneInfo;

            bboxMax = Vector3D.Max(bboxMax, pos);
            bboxMin = Vector3D.Min(bboxMin, pos);
        }
        Array.Resize(ref morphPositions, vertexCount);
        Array.Resize(ref morphUVs, vertexCount);
        Array.Resize(ref updateNormals, vertexCount);
        Array.Resize(ref updateUVs, vertexCount);

        indexElementSize = pmx.Header.VertexIndexSize;
        Array.Resize(ref indices, pmx.Faces.Count * 3 * indexElementSize);
        indexCount = pmx.Faces.Count * 3;
        switch (indexElementSize)
        {
            case 1:
                {
                    int idx = 0;
                    Span<byte> bytes = MemoryMarshal.Cast<char, byte>(indices);
                    foreach (Face face in pmx.Faces)
                    {
                        bytes[idx++] = (byte)face.Vertices[2];
                        bytes[idx++] = (byte)face.Vertices[1];
                        bytes[idx++] = (byte)face.Vertices[0];
                    }
                }
                break;
            case 2:
                {
                    int idx = 0;
                    Span<ushort> ushorts = MemoryMarshal.Cast<char, ushort>(indices);
                    foreach (Face face in pmx.Faces)
                    {
                        ushorts[idx++] = (ushort)face.Vertices[2];
                        ushorts[idx++] = (ushort)face.Vertices[1];
                        ushorts[idx++] = (ushort)face.Vertices[0];
                    }
                }
                break;
            case 4:
                {
                    int idx = 0;
                    Span<uint> uints = MemoryMarshal.Cast<char, uint>(indices);
                    foreach (Face face in pmx.Faces)
                    {
                        uints[idx++] = face.Vertices[2];
                        uints[idx++] = face.Vertices[1];
                        uints[idx++] = face.Vertices[0];
                    }
                }
                break;
            default:
                Console.WriteLine("Unknown IndexSize");
                return false;
        }

        string[] texturePaths = pmx.Textures.Select(x => Path.Combine(dirPath, x.Name)).ToArray();

        // Materialをコピー
        Array.Resize(ref materials, pmx.Materials.Count);
        Array.Resize(ref subMeshes, pmx.Materials.Count);
        uint beginIndex = 0;
        for (int i = 0; i < pmx.Materials.Count; i++)
        {
            Material pmxMat = pmx.Materials[i];

            MMDMaterial mat = new()
            {
                Diffuse = pmxMat.Diffuse.ToVector3D(),
                Alpha = pmxMat.Diffuse.W,
                Specular = pmxMat.Specular,
                SpecularPower = pmxMat.SpecularPower,
                Ambient = pmxMat.Ambient,
                SpTextureMode = SphereTextureMode.None,
                BothFace = pmxMat.DrawMode.HasFlag(DrawModeFlags.BothFace),
                EdgeFlag = Convert.ToByte(pmxMat.DrawMode.HasFlag(DrawModeFlags.DrawEdge)),
                GroundShadow = pmxMat.DrawMode.HasFlag(DrawModeFlags.GroundShadow),
                ShadowCaster = pmxMat.DrawMode.HasFlag(DrawModeFlags.CastSelfShadow),
                ShadowReceiver = pmxMat.DrawMode.HasFlag(DrawModeFlags.RecieveSelfShadow),
                EdgeSize = pmxMat.EdgeSize,
                EdgeColor = pmxMat.EdgeColor
            };

            // Texture
            if (pmxMat.TextureIndex != -1)
            {
                mat.Texture = texturePaths[pmxMat.TextureIndex]!;
            }

            // ToonTexture
            if (pmxMat.ToonMode == ToonMode.Common)
            {
                if (pmxMat.ToonTextureIndex != -1)
                {
                    string texName = pmxMat.ToonTextureIndex + 1 < 10 ? $"0{pmxMat.ToonTextureIndex + 1}" : $"{pmxMat.ToonTextureIndex + 1}";

                    mat.ToonTexture = $"{mmdDataDir}/toon{texName}.bmp";
                }
            }
            else if (pmxMat.ToonMode == ToonMode.Separate)
            {
                if (pmxMat.ToonTextureIndex != -1)
                {
                    mat.ToonTexture = texturePaths[pmxMat.ToonTextureIndex]!;
                }
            }

            // SpTexture
            if (pmxMat.SphereTextureIndex != -1)
            {
                mat.SpTexture = texturePaths[pmxMat.SphereTextureIndex]!;
                mat.SpTextureMode = SphereTextureMode.None;
                if (pmxMat.SphereMode == SphereMode.Mul)
                {
                    mat.SpTextureMode = SphereTextureMode.Mul;
                }
                else if (pmxMat.SphereMode == SphereMode.Add)
                {
                    mat.SpTextureMode = SphereTextureMode.Add;
                }
                else if (pmxMat.SphereMode == SphereMode.SubTexture)
                {
                    // TODO: SphareTexture が SubTexture の処理
                }
            }

            materials[i] = mat;

            MMDSubMesh subMesh = new()
            {
                BeginIndex = (int)beginIndex,
                VertexCount = pmxMat.FaceCount,
                MaterialID = i
            };
            subMeshes[i] = subMesh;

            beginIndex = (uint)(beginIndex + pmxMat.FaceCount);
        }
        initMaterials = materials.ToArray();
        Array.Resize(ref mulMaterialFactors, materials.Length);
        Array.Resize(ref addMaterialFactors, materials.Length);

        // Node
        nodeMan = new MMDNodeManagerT<PMXNode>();
        foreach (Bone bone in pmx.Bones)
        {
            PMXNode node = nodeMan.AddNode();
            node.Name = bone.Name;
        }
        for (int i = 0; i < pmx.Bones.Count; i++)
        {
            int boneIndex = pmx.Bones.Count - i - 1;
            Bone bone = pmx.Bones[boneIndex];
            PMXNode node = nodeMan.Nodes[boneIndex];

            // Check if the node is looping
            bool isLooping = false;
            if (bone.ParentBoneIndex != -1)
            {
                MMDNode? parent = nodeMan.GetMMDNode(bone.ParentBoneIndex);
                while (parent != null)
                {
                    if (parent == node)
                    {
                        isLooping = true;
                        break;
                    }
                    parent = parent.Parent;
                }
            }

            // Check parent node index
            if (bone.ParentBoneIndex != -1)
            {
                if (bone.ParentBoneIndex >= boneIndex)
                {
                    Console.WriteLine($"The parent index of this node is big: bone={boneIndex}");
                }
            }

            if (bone.ParentBoneIndex != -1 && isLooping)
            {
                Bone parentBone = pmx.Bones[bone.ParentBoneIndex];
                MMDNode parent = nodeMan.GetMMDNode(bone.ParentBoneIndex);
                parent.AddChild(node);
                Vector3D<float> localPos = bone.Position - parentBone.Position;
                localPos.Z *= -1;
                node.Translate = localPos;
            }
            else
            {
                Vector3D<float> localPos = bone.Position;
                localPos.Z *= -1;
                node.Translate = localPos;
            }
            Matrix4X4<float> init = Matrix4X4.CreateTranslation(new Vector3D<float>(1.0f, 1.0f, -1.0f) * bone.Position);
            node.GlobalTransform = init;
            node.CalculateInverseInitTransform();

            node.DeformDepth = bone.DeformDepth;
            bool deformAfterPhysics = bone.BoneFlags.HasFlag(BoneFlags.DeformAfterPhysics);
            node.IsDeformAfterPhysics = deformAfterPhysics;
            bool appendRotate = bone.BoneFlags.HasFlag(BoneFlags.AppendRotate);
            node.IsAppendRotate = appendRotate;
            bool appendTranslate = bone.BoneFlags.HasFlag(BoneFlags.AppendTranslate);
            node.IsAppendTranslate = appendTranslate;
            if ((appendRotate || appendTranslate) && bone.AppendBoneIndex != -1)
            {
                if (bone.AppendBoneIndex >= boneIndex)
                {
                    Console.WriteLine($"The parent(morph assignment) index of this node is big: bone={boneIndex}");
                }
                bool appendLocal = bone.BoneFlags.HasFlag(BoneFlags.AppendLocal);
                PMXNode appendNode = nodeMan.GetMMDNode(bone.AppendBoneIndex);
                float appendWeight = bone.AppendWeight;
                node.IsAppendLocal = appendLocal;
                node.AppendNode = appendNode;
                node.AppendWeight = appendWeight;
            }
            node.SaveInitialTRS();
        }
        Array.Resize(ref transforms, nodeMan.GetNodeCount());

        sortedNodes = nodeMan.Nodes.ToArray();
        Array.Sort(sortedNodes, (a, b) => a.DeformDepth - b.DeformDepth);

        // IK
        ikSolverMan = new MMDIkManagerT<MMDIkSolver>();
        for (int i = 0; i < pmx.Bones.Count; i++)
        {
            Bone bone = pmx.Bones[i];
            if (bone.BoneFlags.HasFlag(BoneFlags.IK))
            {
                MMDIkSolver solver = ikSolverMan!.AddIkSolver();
                PMXNode ikNode = nodeMan.GetMMDNode(i);
                solver.IKNode = ikNode;
                ikNode.IkSolver = solver;

                if (bone.IKTargetBoneIndex < 0 || bone.IKTargetBoneIndex >= nodeMan.GetNodeCount())
                {
                    Console.WriteLine($"Wrong IK Target: bone={i} target={bone.IKTargetBoneIndex}");
                    continue;
                }

                PMXNode targetNode = nodeMan.GetMMDNode(bone.IKTargetBoneIndex);
                solver.TargetNode = targetNode;

                foreach (IKLink ikLink in bone.IKLinks)
                {
                    var linkNode = nodeMan.GetMMDNode(ikLink.BoneIndex);
                    if (ikLink.EnableLimit)
                    {
                        Vector3D<float> limitMax = ikLink.LowerLimit * new Vector3D<float>(-1.0f);
                        Vector3D<float> limitMin = ikLink.UpperLimit * new Vector3D<float>(-1.0f);
                        solver.AddIKChain(linkNode, true, limitMin, limitMax);
                    }
                    else
                    {
                        solver.AddIKChain(linkNode);
                    }
                    linkNode.EnableIK = true;
                }

                solver.IterateCount = (uint)bone.IKIterationCount;
                solver.LimitAngle = bone.IKLimit;
            }
        }

        // Morph
        morphMan = new MMDMorphManagerT<PMXMorph>();
        foreach (Morph pmxMorph in pmx.Morphs)
        {
            PMXMorph morph = morphMan.AddMorph();
            morph.Name = pmxMorph.Name;
            morph.Weight = 0.0f;
            morph.MorphType = MorphType.None;
            if (pmxMorph.MorphType == PMX.MorphType.Position)
            {
                morph.MorphType = MorphType.Position;
                morph.DataIndex = positionMorphDatas.Count;
                PositionMorphData morphData = new();
                foreach (PMX.PositionMorph vtx in pmxMorph.PositionMorphs)
                {
                    PositionMorph morphVtx = new()
                    {
                        Index = (uint)vtx.VertexIndex,
                        Position = vtx.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f)
                    };
                    morphData.MorphVertices.Add(morphVtx);
                }
                positionMorphDatas.Add(morphData);
            }
            else if (pmxMorph.MorphType == PMX.MorphType.UV)
            {
                morph.MorphType = MorphType.UV;
                morph.DataIndex = uvMorphDatas.Count;
                UVMorphData morphData = new();
                foreach (PMX.UVMorph uv in pmxMorph.UVMorphs)
                {
                    UVMorph morphUV;
                    morphUV.Index = (uint)uv.VertexIndex;
                    morphUV.UV = uv.UV;
                    morphData.MorphUVs.Add(morphUV);
                }
                uvMorphDatas.Add(morphData);
            }
            else if (pmxMorph.MorphType == PMX.MorphType.Material)
            {
                morph.MorphType = MorphType.Material;
                morph.DataIndex = materialMorphDatas.Count;

                MaterialMorphData materialMorphData = new()
                {
                    MaterialMorphs = pmxMorph.MaterialMorphs.ToArray()
                };
                materialMorphDatas.Add(materialMorphData);
            }
            else if (pmxMorph.MorphType == PMX.MorphType.Bone)
            {
                morph.MorphType = MorphType.Bone;
                morph.DataIndex = boneMorphDatas.Count;

                BoneMorphData boneMorphData = new();
                foreach (BoneMorph pmxBoneMorphElem in pmxMorph.BoneMorphs)
                {
                    BoneMorphElement boneMorphElem;
                    boneMorphElem.Node = nodeMan.GetMMDNode(pmxBoneMorphElem.BoneIndex);
                    boneMorphElem.Position = pmxBoneMorphElem.Position * new Vector3D<float>(1.0f, 1.0f, -1.0f);
                    Quaternion<float> q = pmxBoneMorphElem.Quaternion;
                    Matrix4X4<float> invZ = Matrix4X4.CreateScale(new Vector3D<float>(1.0f, 1.0f, -1.0f));
                    Matrix4X4<float> rot0 = Matrix4X4.CreateFromQuaternion(q);
                    Matrix4X4<float> rot1 = invZ * rot0 * invZ;
                    boneMorphElem.Rotate = Quaternion<float>.CreateFromRotationMatrix(rot1);
                    boneMorphData.BoneMorphs.Add(boneMorphElem);
                }
                boneMorphDatas.Add(boneMorphData);
            }
            else if (pmxMorph.MorphType == PMX.MorphType.Group)
            {
                morph.MorphType = MorphType.Group;
                morph.DataIndex = groupMorphDatas.Count;

                GroupMorphData groupMorphData = new()
                {
                    GroupMorphs = pmxMorph.GroupMorphs.ToArray()
                };
                groupMorphDatas.Add(groupMorphData);
            }
            else
            {
                Console.WriteLine($"Not Supported Morp Type({pmxMorph.MorphType}): [{pmxMorph.Name}]");
            }
        }

        // Check whether Group Morph infinite loop.
        {
            List<int> groupMorphStack = new();
            void FixInifinitGropuMorph(int morphIdx)
            {
                List<PMXMorph> morphs = morphMan.Morphs;
                PMXMorph morph = morphs[morphIdx];

                if (morph.MorphType == MorphType.Group)
                {
                    GroupMorphData groupMorphData = groupMorphDatas[morph.DataIndex];
                    for (int i = 0; i < groupMorphData.GroupMorphs.Length; i++)
                    {
                        GroupMorph groupMorph = groupMorphData.GroupMorphs[i];

                        int findIt = groupMorphStack.IndexOf(groupMorph.MorphIndex);
                        if (findIt != -1)
                        {
                            Console.WriteLine($"Infinit Group Morph:[{morphIdx}][{morph.Name}][{i}]");

                            groupMorph.MorphIndex = -1;
                        }
                        else
                        {
                            groupMorphStack.Add(morphIdx);
                            if (groupMorph.MorphIndex > 0)
                            {
                                FixInifinitGropuMorph(groupMorph.MorphIndex);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid morph index: group={groupMorph.MorphIndex}, morph={morphIdx}");
                            }
                            groupMorphStack.RemoveAt(groupMorphStack.Count - 1);
                        }
                    }
                }
            }

            for (int morphIdx = 0; morphIdx < morphMan.GetMorphCount(); morphIdx++)
            {
                FixInifinitGropuMorph(morphIdx);
                groupMorphStack.Clear();
            }
        }

        // Physics
        physicsMan = new MMDPhysicsManager();
        if (!physicsMan.Create())
        {
            Console.WriteLine("Create Physics Fail.");
            return false;
        }

        foreach (Rigidbody pmxRB in pmx.Rigidbodies)
        {
            MMDRigidBody rb = physicsMan.AddRigidBody();
            MMDNode? node = null;
            if (pmxRB.BoneIndex != -1)
            {
                node = nodeMan.GetMMDNode(pmxRB.BoneIndex);
            }
            if (!rb.Create(pmxRB, this, node!))
            {
                Console.WriteLine("Create Rigid Body Fail.");
                return false;
            }
            physicsMan.MMDPhysics!.AddRigidBody(rb);
        }

        foreach (Joint pmxJoint in pmx.Joints)
        {
            if (pmxJoint.RigidBodyIndexA != -1
                && pmxJoint.RigidBodyIndexB != -1
                && pmxJoint.RigidBodyIndexA != pmxJoint.RigidBodyIndexB)
            {
                MMDJoint joint = physicsMan.AddJoint();
                List<MMDRigidBody> rigidBodys = physicsMan.RigidBodys;
                bool ret = joint.CreateJoint(pmxJoint,
                                             rigidBodys[pmxJoint.RigidBodyIndexA],
                                             rigidBodys[pmxJoint.RigidBodyIndexB]);
                if (!ret)
                {
                    Console.WriteLine("Create Joint Fail.");
                    return false;
                }
                physicsMan.MMDPhysics!.AddJoint(joint);
            }
            else
            {
                Console.WriteLine($"Illegal Joint [{pmxJoint.Name}]");
            }
        }

        ResetPhysics();

        SetupParallelUpdate();

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
        Span<Vector3D<float>> position = positions.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector3D<float>> normal = normals.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector2D<float>> uv = uvs.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector3D<float>> morphPos = morphPositions.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector4D<float>> morphUV = morphUVs.AsSpan(range.VertexOffset, range.VertexCount);
        Span<VertexBoneInfo> vtxInfo = vertexBoneInfos.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Matrix4X4<float>> transforms = this.transforms.AsSpan();
        Span<Vector3D<float>> updatePosition = updatePositions.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector3D<float>> updateNormal = updateNormals.AsSpan(range.VertexOffset, range.VertexCount);
        Span<Vector2D<float>> updateUV = updateUVs.AsSpan(range.VertexOffset, range.VertexCount);

        for (int i = 0; i < range.VertexCount; i++)
        {
            Matrix4X4<float> m = Matrix4X4<float>.Identity;
            switch (vtxInfo[i].SkinningType)
            {
                case SkinningType.Weight1:
                    {
                        int i0 = vtxInfo[i].BoneIndex[0];
                        Matrix4X4<float> m0 = transforms[i0];
                        m = m0;
                    }
                    break;
                case SkinningType.Weight2:
                    {
                        int i0 = vtxInfo[i].BoneIndex[0];
                        int i1 = vtxInfo[i].BoneIndex[1];
                        float w0 = vtxInfo[i].BoneWeight[0];
                        float w1 = vtxInfo[i].BoneWeight[1];
                        Matrix4X4<float> m0 = transforms[i0];
                        Matrix4X4<float> m1 = transforms[i1];
                        m = m0 * w0 + m1 * w1;
                    }
                    break;
                case SkinningType.Weight4:
                    {
                        int i0 = vtxInfo[i].BoneIndex[0];
                        int i1 = vtxInfo[i].BoneIndex[1];
                        int i2 = vtxInfo[i].BoneIndex[2];
                        int i3 = vtxInfo[i].BoneIndex[3];
                        float w0 = vtxInfo[i].BoneWeight[0];
                        float w1 = vtxInfo[i].BoneWeight[1];
                        float w2 = vtxInfo[i].BoneWeight[2];
                        float w3 = vtxInfo[i].BoneWeight[3];
                        Matrix4X4<float> m0 = transforms[i0];
                        Matrix4X4<float> m1 = transforms[i1];
                        Matrix4X4<float> m2 = transforms[i2];
                        Matrix4X4<float> m3 = transforms[i3];
                        m = m0 * w0 + m1 * w1 + m2 * w2 + m3 * w3;
                    }
                    break;
                case SkinningType.SDEF:
                    {
                        // https://github.com/powroupi/blender_mmd_tools/blob/dev_test/mmd_tools/core/sdef.py

                        List<PMXNode> nodes = nodeMan!.Nodes;
                        int i0 = vtxInfo[i].SDEF.BoneIndex[0];
                        int i1 = vtxInfo[i].SDEF.BoneIndex[1];
                        float w0 = vtxInfo[i].SDEF.BoneWeight;
                        float w1 = 1.0f - w0;
                        Vector3D<float> center = vtxInfo[i].SDEF.C;
                        Vector3D<float> cr0 = vtxInfo[i].SDEF.R0;
                        Vector3D<float> cr1 = vtxInfo[i].SDEF.R1;
                        Quaternion<float> q0 = Quaternion<float>.CreateFromRotationMatrix(nodes[i0].GlobalTransform);
                        Quaternion<float> q1 = Quaternion<float>.CreateFromRotationMatrix(nodes[i1].GlobalTransform);
                        Matrix4X4<float> m0 = transforms[i0];
                        Matrix4X4<float> m1 = transforms[i1];

                        Vector3D<float> pos = position[i] + morphPos[i];
                        Matrix3X3<float> rot_mat = Matrix3X3.CreateFromQuaternion(Quaternion<float>.Slerp(q0, q1, w1));

                        updatePosition[i] = Vector3D.Multiply(pos - center, rot_mat) + Vector4D.Multiply(new Vector4D<float>(cr0, 1.0f), m0).ToVector3D() + Vector4D.Multiply(new Vector4D<float>(cr1, 1.0f), m1).ToVector3D();
                        updateNormal[i] = Vector3D.Multiply(normal[i], rot_mat);
                    }
                    break;
                case SkinningType.DualQuaternion:
                    {
                        //
                        // Skinning with Dual Quaternions
                        // https://www.cs.utah.edu/~ladislav/dq/index.html
                        //

                    }
                    break;
                default:
                    break;
            }

            if (vtxInfo[i].SkinningType != SkinningType.SDEF)
            {
                updatePosition[i] = Vector4D.Multiply(new Vector4D<float>(position[i] + morphPos[i], 1.0f), m).ToVector3D();
                updateNormal[i] = Vector4D.Normalize(Vector4D.Multiply(new Vector4D<float>(normal[i], 1.0f), m)).ToVector3D();
            }
            updateUV[i] = uv[i] + new Vector2D<float>(morphUV[i].X, morphUV[i].Y);
        }
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

    public override MMDMaterial[] GetMaterials()
    {
        return materials;
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