using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

#region Enums
public enum VertexWeight : byte
{
    BDEF1,
    BDEF2,
    BDEF4,
    SDEF,
    QDEF
}

public enum DrawModeFlags : byte
{
    BothFace = 0x01,
    GroundShadow = 0x02,
    CastSelfShadow = 0x04,
    RecieveSelfShadow = 0x08,
    DrawEdge = 0x10,
    VertexColor = 0x20,
    DrawPoint = 0x40,
    DrawLine = 0x80
}

public enum SphereMode : byte
{
    None,
    Mul,
    Add,
    SubTexture
}

public enum ToonMode : byte
{
    Separate,
    Common
}

public enum BoneFlags : ushort
{
    TargetShowMode = 0x0001,
    AllowRotate = 0x0002,
    AllowTranslate = 0x0004,
    Visible = 0x0008,
    AllowControl = 0x0010,
    IK = 0x0020,
    AppendLocal = 0x0080,
    AppendRotate = 0x0100,
    AppendTranslate = 0x0200,
    FixedAxis = 0x0400,
    LocalAxis = 0x800,
    DeformAfterPhysics = 0x1000,
    DeformOuterParent = 0x2000
}

public enum MorphType : byte
{
    Group,
    Position,
    Bone,
    UV,
    AddUV1,
    AddUV2,
    AddUV3,
    AddUV4,
    Material,
    Flip,
    Impluse
}

public enum MorphCategory : byte
{
    System,
    Eyebrow,
    Eye,
    Mouth,
    Other
}

public enum OpType : byte
{
    Mul,
    Add
}

public enum TargetType : byte
{
    BoneIndex,
    MorphIndex
}

public enum FrameType : byte
{
    DefaultFrame,
    SpecialFrame
}
#endregion

#region Structs
public struct Header
{
    public string Magic;

    public float Version;

    public byte DataSize;

    public EncodingType Encoding;

    public byte AdditionalUV;

    public byte VertexIndexSize;

    public byte TextureIndexSize;

    public byte MaterialIndexSize;

    public byte BoneIndexSize;

    public byte MorphIndexSize;

    public byte RigidBodyIndexSize;
}

public struct Info
{
    public string ModelName;

    public string ModelNameEn;

    public string Comment;

    public string CommentEn;
}

public unsafe struct Vertex
{
    public Vector3D<float> Position;

    public Vector3D<float> Normal;

    public Vector2D<float> UV;

    public fixed float AdditionalUV[4 * 4];

    public VertexWeight WeightType;

    public fixed int BoneIndices[4];

    public fixed float BoneWeights[4];

    public Vector3D<float> SdefC;

    public Vector3D<float> SdefR0;

    public Vector3D<float> SdefR1;

    public float EdgeScale;

    public readonly Vector4D<float> GetAdditionalUV(int index)
    {
        fixed (float* additionalUV = AdditionalUV)
        {
            return *(Vector4D<float>*)(additionalUV + index * 4);
        }
    }

    public readonly void SetAdditionalUV(int index, Vector4D<float> value)
    {
        fixed (float* additionalUV = AdditionalUV)
        {
            *(Vector4D<float>*)(additionalUV + index * 4) = value;
        }
    }
}

public unsafe struct Face
{
    public fixed uint Vertices[3];
}

public struct Texture
{
    public string Name;
}

public struct Material
{
    public string Name;

    public string NameEn;

    public Vector4D<float> Diffuse;

    public Vector3D<float> Specular;

    public float SpecularPower;

    public Vector3D<float> Ambient;

    public DrawModeFlags DrawMode;

    public Vector4D<float> EdgeColor;

    public float EdgeSize;

    public int TextureIndex;

    public int SphereTextureIndex;

    public SphereMode SphereMode;

    public ToonMode ToonMode;

    public int ToonTextureIndex;

    public string Memo;

    public int FaceCount;
}

public struct IKLink
{
    public int BoneIndex;

    public bool EnableLimit;

    public Vector3D<float> LowerLimit;

    public Vector3D<float> UpperLimit;
}

public struct Bone
{
    public string Name;

    public string NameEn;

    public Vector3D<float> Position;

    public int ParentBoneIndex;

    public int DeformDepth;

    public BoneFlags BoneFlags;

    public Vector3D<float> PositionOffset;

    public int LinkBoneIndex;

    public int AppendBoneIndex;

    public float AppendWeight;

    public Vector3D<float> FixedAxis;

    public Vector3D<float> LocalAxisX;

    public Vector3D<float> LocalAxisZ;

    public int KeyValue;

    public int IKTargetBoneIndex;

    public int IKIterationCount;

    public float IKLimit;

    public IKLink[] IKLinks;
}

public struct PositionMorph
{
    public int VertexIndex;

    public Vector3D<float> Position;
}

public struct UVMorph
{
    public int VertexIndex;

    public Vector4D<float> UV;
}

public struct BoneMorph
{
    public int BoneIndex;

    public Vector3D<float> Position;

    public Quaternion<float> Quaternion;
}

public struct MaterialMorph
{
    public int MaterialIndex;

    public OpType OpType;

    public Vector4D<float> Diffuse;

    public Vector3D<float> Specular;

    public float SpecularPower;

    public Vector3D<float> Ambient;

    public Vector4D<float> EdgeColor;

    public float EdgeSize;

    public Vector4D<float> TextureCoefficient;

    public Vector4D<float> SphereTextureCoefficient;

    public Vector4D<float> ToonTextureCoefficient;
}

public struct GroupMorph
{
    public int MorphIndex;

    public float Weight;
}

public struct FlipMorph
{
    public int MorphIndex;

    public float Weight;
}

public struct ImpulseMorph
{
    public int RigidBodyIndex;

    public bool LocalFlag;

    public Vector3D<float> Velocity;

    public Vector3D<float> Torque;
}

public struct Morph
{
    public string Name;

    public string NameEn;

    public MorphCategory ControlPanel;

    public MorphType MorphType;

    public PositionMorph[] PositionMorphs;

    public UVMorph[] UVMorphs;

    public BoneMorph[] BoneMorphs;

    public MaterialMorph[] MaterialMorphs;

    public GroupMorph[] GroupMorphs;

    public FlipMorph[] FlipMorphs;

    public ImpulseMorph[] ImpulseMorphs;
}

public struct Target
{
    public TargetType Type;

    public int Index;
}

public struct DisplayFrame
{
    public string Name;

    public string NameEn;

    public FrameType Flag;

    public Target[] Targets;
}
#endregion

public unsafe class PMXFile
{
    public Header Header { get; private set; }

    public Info Info { get; private set; }

    public List<Vertex> Vertices { get; } = new List<Vertex>();

    public List<Face> Faces { get; } = new List<Face>();

    public List<Texture> Textures { get; } = new List<Texture>();

    public List<Material> Materials { get; } = new List<Material>();

    public List<Bone> Bones { get; } = new List<Bone>();

    public List<Morph> Morphs { get; } = new List<Morph>();

    public List<DisplayFrame> DisplayFrames { get; } = new List<DisplayFrame>();

    public PMXFile(string file)
    {
        using BinaryReader binaryReader = new(File.OpenRead(file));

        ReadHeader(this, binaryReader);
        ReadInfo(this, binaryReader);
        ReadVertex(this, binaryReader);
        ReadFace(this, binaryReader);
        ReadTexture(this, binaryReader);
        ReadMaterial(this, binaryReader);
        ReadBone(this, binaryReader);
        ReadMorph(this, binaryReader);
        ReadDisplayFrame(this, binaryReader);
    }

    private static void ReadHeader(PMXFile pmx, BinaryReader binaryReader)
    {
        Header header = new()
        {
            Magic = binaryReader.ReadString(4),
            Version = binaryReader.ReadSingle(),
            DataSize = binaryReader.ReadByte(),
            Encoding = binaryReader.ReadByte() == 0 ? EncodingType.Unicode : EncodingType.UTF8,
            AdditionalUV = binaryReader.ReadByte(),
            VertexIndexSize = binaryReader.ReadByte(),
            TextureIndexSize = binaryReader.ReadByte(),
            MaterialIndexSize = binaryReader.ReadByte(),
            BoneIndexSize = binaryReader.ReadByte(),
            MorphIndexSize = binaryReader.ReadByte(),
            RigidBodyIndexSize = binaryReader.ReadByte()
        };

        pmx.Header = header;
    }

    private static void ReadInfo(PMXFile pmx, BinaryReader binaryReader)
    {
        Info info = new()
        {
            ModelName = binaryReader.ReadString(pmx.Header.Encoding),
            ModelNameEn = binaryReader.ReadString(pmx.Header.Encoding),
            Comment = binaryReader.ReadString(pmx.Header.Encoding),
            CommentEn = binaryReader.ReadString(pmx.Header.Encoding)
        };

        pmx.Info = info;
    }

    private static void ReadVertex(PMXFile pmx, BinaryReader binaryReader)
    {
        int vertexCount = binaryReader.ReadInt32();
        pmx.Vertices.Resize(vertexCount);

        for (int i = 0; i < vertexCount; i++)
        {
            Vertex vertex = new()
            {
                Position = binaryReader.ReadVector3D(),
                Normal = binaryReader.ReadVector3D(),
                UV = binaryReader.ReadVector2D(),
            };

            for (byte j = 0; j < pmx.Header.AdditionalUV; j++)
            {
                vertex.SetAdditionalUV(j, binaryReader.ReadVector4D());
            }

            vertex.WeightType = (VertexWeight)binaryReader.ReadByte();

            switch (vertex.WeightType)
            {
                case VertexWeight.BDEF1:
                    vertex.BoneIndices[0] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    break;
                case VertexWeight.BDEF2:
                    vertex.BoneIndices[0] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[1] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneWeights[0] = binaryReader.ReadSingle();
                    break;
                case VertexWeight.BDEF4:
                    vertex.BoneIndices[0] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[1] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[2] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[3] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneWeights[0] = binaryReader.ReadSingle();
                    vertex.BoneWeights[1] = binaryReader.ReadSingle();
                    vertex.BoneWeights[2] = binaryReader.ReadSingle();
                    vertex.BoneWeights[3] = binaryReader.ReadSingle();
                    break;
                case VertexWeight.SDEF:
                    vertex.BoneIndices[0] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[1] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneWeights[0] = binaryReader.ReadSingle();
                    vertex.SdefC = binaryReader.ReadVector3D();
                    vertex.SdefR0 = binaryReader.ReadVector3D();
                    vertex.SdefR1 = binaryReader.ReadVector3D();
                    break;
                case VertexWeight.QDEF:
                    vertex.BoneIndices[0] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[1] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[2] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneIndices[3] = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                    vertex.BoneWeights[0] = binaryReader.ReadSingle();
                    vertex.BoneWeights[1] = binaryReader.ReadSingle();
                    vertex.BoneWeights[2] = binaryReader.ReadSingle();
                    vertex.BoneWeights[3] = binaryReader.ReadSingle();
                    break;
                default: break;
            }

            vertex.EdgeScale = binaryReader.ReadSingle();

            pmx.Vertices[i] = vertex;
        }
    }

    private static void ReadFace(PMXFile pmx, BinaryReader binaryReader)
    {
        int faceCount = binaryReader.ReadInt32() / 3;
        pmx.Faces.Resize(faceCount);

        switch (pmx.Header.VertexIndexSize)
        {
            case 1:
                {
                    byte[] vertices = binaryReader.ReadBytes(faceCount * 3);
                    for (int i = 0; i < faceCount; i++)
                    {
                        Face face = new();
                        face.Vertices[0] = vertices[i * 3];
                        face.Vertices[1] = vertices[i * 3 + 1];
                        face.Vertices[2] = vertices[i * 3 + 2];

                        pmx.Faces[i] = face;
                    }
                }
                break;
            case 2:
                {
                    ushort[] vertices = binaryReader.ReadUInt16s(faceCount * 3);
                    for (int i = 0; i < faceCount; i++)
                    {
                        Face face = new();
                        face.Vertices[0] = vertices[i * 3];
                        face.Vertices[1] = vertices[i * 3 + 1];
                        face.Vertices[2] = vertices[i * 3 + 2];

                        pmx.Faces[i] = face;
                    }
                }
                break;
            case 4:
                {
                    uint[] vertices = binaryReader.ReadUInt32s(faceCount * 3);
                    for (int i = 0; i < faceCount; i++)
                    {
                        Face face = new();
                        face.Vertices[0] = vertices[i * 3];
                        face.Vertices[1] = vertices[i * 3 + 1];
                        face.Vertices[2] = vertices[i * 3 + 2];

                        pmx.Faces[i] = face;
                    }
                }
                break;
            default: break;
        }
    }

    private static void ReadTexture(PMXFile pmx, BinaryReader binaryReader)
    {
        int textureCount = binaryReader.ReadInt32();
        pmx.Textures.Resize(textureCount);

        for (int i = 0; i < textureCount; i++)
        {
            Texture texture = new()
            {
                Name = binaryReader.ReadString(pmx.Header.Encoding)
            };

            pmx.Textures[i] = texture;
        }
    }

    private static void ReadMaterial(PMXFile pmx, BinaryReader binaryReader)
    {
        int materialCount = binaryReader.ReadInt32();
        pmx.Materials.Resize(materialCount);

        for (int i = 0; i < materialCount; i++)
        {
            Material material = new()
            {
                Name = binaryReader.ReadString(pmx.Header.Encoding),
                NameEn = binaryReader.ReadString(pmx.Header.Encoding),
                Diffuse = binaryReader.ReadVector4D(),
                Specular = binaryReader.ReadVector3D(),
                SpecularPower = binaryReader.ReadSingle(),
                Ambient = binaryReader.ReadVector3D(),
                DrawMode = (DrawModeFlags)binaryReader.ReadByte(),
                EdgeColor = binaryReader.ReadVector4D(),
                EdgeSize = binaryReader.ReadSingle(),
                TextureIndex = binaryReader.ReadIndex(pmx.Header.TextureIndexSize),
                SphereTextureIndex = binaryReader.ReadIndex(pmx.Header.TextureIndexSize),
                SphereMode = (SphereMode)binaryReader.ReadByte(),
                ToonMode = (ToonMode)binaryReader.ReadByte()
            };
            if (material.ToonMode == ToonMode.Separate)
            {
                material.ToonTextureIndex = binaryReader.ReadIndex(pmx.Header.TextureIndexSize);
            }
            else if (material.ToonMode == ToonMode.Common)
            {
                material.ToonTextureIndex = binaryReader.ReadByte();
            }
            else
            {
                material.ToonTextureIndex = -1;
            }

            material.Memo = binaryReader.ReadString(pmx.Header.Encoding);
            material.FaceCount = binaryReader.ReadInt32();

            pmx.Materials[i] = material;
        }
    }

    private static void ReadBone(PMXFile pmx, BinaryReader binaryReader)
    {
        int boneCount = binaryReader.ReadInt32();
        pmx.Bones.Resize(boneCount);

        for (int i = 0; i < boneCount; i++)
        {
            Bone bone = new()
            {
                Name = binaryReader.ReadString(pmx.Header.Encoding),
                NameEn = binaryReader.ReadString(pmx.Header.Encoding),
                Position = binaryReader.ReadVector3D(),
                ParentBoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize),
                DeformDepth = binaryReader.ReadInt32(),
                BoneFlags = (BoneFlags)binaryReader.ReadUInt16()
            };

            if (bone.BoneFlags.HasFlag(BoneFlags.TargetShowMode))
            {
                bone.LinkBoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
            }
            else
            {
                bone.PositionOffset = binaryReader.ReadVector3D();
            }

            if (bone.BoneFlags.HasFlag(BoneFlags.AppendRotate) | bone.BoneFlags.HasFlag(BoneFlags.AppendTranslate))
            {
                bone.AppendBoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                bone.AppendWeight = binaryReader.ReadSingle();
            }

            if (bone.BoneFlags.HasFlag(BoneFlags.FixedAxis))
            {
                bone.FixedAxis = binaryReader.ReadVector3D();
            }

            if (bone.BoneFlags.HasFlag(BoneFlags.LocalAxis))
            {
                bone.LocalAxisX = binaryReader.ReadVector3D();
                bone.LocalAxisZ = binaryReader.ReadVector3D();
            }

            if (bone.BoneFlags.HasFlag(BoneFlags.DeformOuterParent))
            {
                bone.KeyValue = binaryReader.ReadInt32();
            }

            if (bone.BoneFlags.HasFlag(BoneFlags.IK))
            {
                bone.IKTargetBoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize);
                bone.IKIterationCount = binaryReader.ReadInt32();
                bone.IKLimit = binaryReader.ReadSingle();

                int ikLinkCount = binaryReader.ReadInt32();

                Array.Resize(ref bone.IKLinks, ikLinkCount);
                for (int j = 0; j < ikLinkCount; j++)
                {
                    IKLink iKLink = new()
                    {
                        BoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize),
                        EnableLimit = binaryReader.ReadBoolean()
                    };

                    if (iKLink.EnableLimit)
                    {
                        iKLink.LowerLimit = binaryReader.ReadVector3D();
                        iKLink.UpperLimit = binaryReader.ReadVector3D();
                    }

                    bone.IKLinks[j] = iKLink;
                }
            }

            pmx.Bones[i] = bone;
        }
    }

    private static void ReadMorph(PMXFile pmx, BinaryReader binaryReader)
    {
        int morphCount = binaryReader.ReadInt32();
        pmx.Morphs.Resize(morphCount);

        for (int i = 0; i < morphCount; i++)
        {
            Morph morph = new()
            {
                Name = binaryReader.ReadString(pmx.Header.Encoding),
                NameEn = binaryReader.ReadString(pmx.Header.Encoding),
                ControlPanel = (MorphCategory)binaryReader.ReadByte(),
                MorphType = (MorphType)binaryReader.ReadByte()
            };

            int dataCount = binaryReader.ReadInt32();

            if (morph.MorphType.HasFlag(MorphType.Position))
            {
                Array.Resize(ref morph.PositionMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    PositionMorph positionMorph = new()
                    {
                        VertexIndex = binaryReader.ReadIndex(pmx.Header.VertexIndexSize),
                        Position = binaryReader.ReadVector3D()
                    };

                    morph.PositionMorphs[j] = positionMorph;
                }
            }
            else if (morph.MorphType is MorphType.UV
                     or MorphType.AddUV1
                     or MorphType.AddUV2
                     or MorphType.AddUV3
                     or MorphType.AddUV4)
            {
                Array.Resize(ref morph.UVMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    UVMorph uVMorph = new()
                    {
                        VertexIndex = binaryReader.ReadIndex(pmx.Header.VertexIndexSize),
                        UV = binaryReader.ReadVector4D()
                    };

                    morph.UVMorphs[j] = uVMorph;
                }
            }
            else if (morph.MorphType == MorphType.Bone)
            {
                Array.Resize(ref morph.BoneMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    BoneMorph boneMorph = new()
                    {
                        BoneIndex = binaryReader.ReadIndex(pmx.Header.BoneIndexSize),
                        Position = binaryReader.ReadVector3D(),
                        Quaternion = binaryReader.ReadQuaternion()
                    };

                    morph.BoneMorphs[j] = boneMorph;
                }
            }
            else if (morph.MorphType == MorphType.Material)
            {
                Array.Resize(ref morph.MaterialMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    MaterialMorph materialMorph = new()
                    {
                        MaterialIndex = binaryReader.ReadIndex(pmx.Header.MaterialIndexSize),
                        OpType = (OpType)binaryReader.ReadByte(),
                        Diffuse = binaryReader.ReadVector4D(),
                        Specular = binaryReader.ReadVector3D(),
                        SpecularPower = binaryReader.ReadSingle(),
                        Ambient = binaryReader.ReadVector3D(),
                        EdgeColor = binaryReader.ReadVector4D(),
                        EdgeSize = binaryReader.ReadSingle(),
                        TextureCoefficient = binaryReader.ReadVector4D(),
                        SphereTextureCoefficient = binaryReader.ReadVector4D(),
                        ToonTextureCoefficient = binaryReader.ReadVector4D()
                    };

                    morph.MaterialMorphs[j] = materialMorph;
                }
            }
            else if (morph.MorphType == MorphType.Group)
            {
                Array.Resize(ref morph.GroupMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    GroupMorph groupMorph = new()
                    {
                        MorphIndex = binaryReader.ReadIndex(pmx.Header.MorphIndexSize),
                        Weight = binaryReader.ReadSingle()
                    };

                    morph.GroupMorphs[j] = groupMorph;
                }
            }
            else if (morph.MorphType == MorphType.Flip)
            {
                Array.Resize(ref morph.FlipMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    FlipMorph flipMorph = new()
                    {
                        MorphIndex = binaryReader.ReadIndex(pmx.Header.MorphIndexSize),
                        Weight = binaryReader.ReadSingle()
                    };

                    morph.FlipMorphs[j] = flipMorph;
                }
            }
            else if (morph.MorphType == MorphType.Impluse)
            {
                Array.Resize(ref morph.ImpulseMorphs, dataCount);
                for (int j = 0; j < dataCount; j++)
                {
                    ImpulseMorph impulseMorph = new()
                    {
                        RigidBodyIndex = binaryReader.ReadIndex(pmx.Header.RigidBodyIndexSize),
                        LocalFlag = binaryReader.ReadBoolean(),
                        Velocity = binaryReader.ReadVector3D(),
                        Torque = binaryReader.ReadVector3D()
                    };

                    morph.ImpulseMorphs[j] = impulseMorph;
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            pmx.Morphs[i] = morph;
        }
    }

    private static void ReadDisplayFrame(PMXFile pmx, BinaryReader binaryReader)
    {
        int displayFrameCount = binaryReader.ReadInt32();
        pmx.DisplayFrames.Resize(displayFrameCount);

        for (int i = 0; i < displayFrameCount; i++)
        {
            DisplayFrame displayFrame = new()
            {
                Name = binaryReader.ReadString(pmx.Header.Encoding),
                NameEn = binaryReader.ReadString(pmx.Header.Encoding),
                Flag = (FrameType)binaryReader.ReadByte()
            };

            int targetCount = binaryReader.ReadInt32();
            Array.Resize(ref displayFrame.Targets, targetCount);

            for (int j = 0; j < targetCount; j++)
            {
                Target target = new()
                {
                    Type = (TargetType)binaryReader.ReadByte()
                };

                target.Index = target.Type switch
                {
                    TargetType.BoneIndex => binaryReader.ReadIndex(pmx.Header.BoneIndexSize),
                    TargetType.MorphIndex => binaryReader.ReadIndex(pmx.Header.MorphIndexSize),
                    _ => throw new NotSupportedException(),
                };

                displayFrame.Targets[j] = target;
            }
        }
    }
}