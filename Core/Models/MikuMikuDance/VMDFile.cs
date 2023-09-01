using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance.VMD;

#region Enums
public enum ShadowType : byte
{
    Off,
    Mode1,
    Mode2,
}
#endregion

#region Structs
public struct Header
{
    public string Title;

    public string ModelName;
}

public struct Motion
{
    public string BoneName;

    public uint Frame;

    public Vector3D<float> Translate;

    public Quaternion<float> Quaternion;

    public byte[] Interpolation;
}

public struct Morph
{
    public string BlendShapeName;

    public uint Frame;

    public float Weight;
}

public unsafe struct Camera
{
    public uint Frame;

    public float Distance;

    public Vector3D<float> Interest;

    public Vector3D<float> Rotate;

    public byte[] Interpolation;

    public uint ViewAngle;

    public bool IsPerspective;
}

public struct Light
{
    public uint Frame;

    public Vector3D<float> Color;

    public Vector3D<float> Position;
}

public struct Shadow
{
    public uint Frame;

    public byte ShadowType;

    public float Distance;
}

public struct IkInfo
{
    public string Name;

    public bool Enable;
}

public struct Ik
{
    public uint Frame;

    public bool Show;

    public IkInfo[] IkInfos;
}
#endregion

public unsafe class VMDFile
{
    public Header Header { get; private set; }

    public List<Motion> Motions { get; } = new List<Motion>();

    public List<Morph> Morphs { get; } = new List<Morph>();

    public List<Camera> Cameras { get; } = new List<Camera>();

    public List<Light> Lights { get; } = new List<Light>();

    public List<Shadow> Shadows { get; } = new List<Shadow>();

    public List<Ik> Iks { get; } = new List<Ik>();

    public VMDFile(string file)
    {
        using BinaryReader binaryReader = new(File.OpenRead(file));

        ReadHeader(this, binaryReader);
        ReadMotion(this, binaryReader);
        ReadMorph(this, binaryReader);
        ReadCamera(this, binaryReader);
        ReadLight(this, binaryReader);
        ReadShadow(this, binaryReader);
        ReadIk(this, binaryReader);
    }

    private static void ReadHeader(VMDFile vmd, BinaryReader binaryReader)
    {
        Header header = new()
        {
            Title = binaryReader.ReadString(30),
            ModelName = binaryReader.ReadString(20)
        };

        if (header.Title != "Vocaloid Motion Data 0002" && header.Title != "Vocaloid Motion Data")
        {
            throw new Exception("Invalid VMD file.");
        }

        vmd.Header = header;
    }

    private static void ReadMotion(VMDFile vmd, BinaryReader binaryReader)
    {
        uint motionCount = binaryReader.ReadUInt32();
        vmd.Motions.Resize(motionCount);

        for (int i = 0; i < motionCount; i++)
        {
            Motion motion = new()
            {
                BoneName = binaryReader.ReadString(15, EncodingType.ShiftJIS),
                Frame = binaryReader.ReadUInt32(),
                Translate = binaryReader.ReadVector3D(),
                Quaternion = binaryReader.ReadQuaternion(),
                Interpolation = binaryReader.ReadBytes(64)
            };

            vmd.Motions[i] = motion;
        }
    }

    private static void ReadMorph(VMDFile vmd, BinaryReader binaryReader)
    {
        uint morphCount = binaryReader.ReadUInt32();
        vmd.Morphs.Resize(morphCount);

        for (int i = 0; i < morphCount; i++)
        {
            Morph morph = new()
            {
                BlendShapeName = binaryReader.ReadString(15, EncodingType.ShiftJIS),
                Frame = binaryReader.ReadUInt32(),
                Weight = binaryReader.ReadSingle()
            };

            vmd.Morphs[i] = morph;
        }
    }

    private static void ReadCamera(VMDFile vmd, BinaryReader binaryReader)
    {
        uint cameraCount = binaryReader.ReadUInt32();
        vmd.Cameras.Resize(cameraCount);

        for (int i = 0; i < cameraCount; i++)
        {
            Camera camera = new()
            {
                Frame = binaryReader.ReadUInt32(),
                Distance = binaryReader.ReadSingle(),
                Interest = binaryReader.ReadVector3D(),
                Rotate = binaryReader.ReadVector3D(),
                Interpolation = binaryReader.ReadBytes(24),
                ViewAngle = binaryReader.ReadUInt32(),
                IsPerspective = binaryReader.ReadBoolean()
            };

            vmd.Cameras[i] = camera;
        }
    }

    private static void ReadLight(VMDFile vmd, BinaryReader binaryReader)
    {
        uint lightCount = binaryReader.ReadUInt32();
        vmd.Lights.Resize(lightCount);

        for (int i = 0; i < lightCount; i++)
        {
            Light light = new()
            {
                Frame = binaryReader.ReadUInt32(),
                Color = binaryReader.ReadVector3D(),
                Position = binaryReader.ReadVector3D()
            };

            vmd.Lights[i] = light;
        }
    }

    private static void ReadShadow(VMDFile vmd, BinaryReader binaryReader)
    {
        uint shadowCount = binaryReader.ReadUInt32();
        vmd.Shadows.Resize(shadowCount);

        for (int i = 0; i < shadowCount; i++)
        {
            Shadow shadow = new()
            {
                Frame = binaryReader.ReadUInt32(),
                ShadowType = binaryReader.ReadByte(),
                Distance = binaryReader.ReadSingle()
            };

            vmd.Shadows[i] = shadow;
        }
    }

    private static void ReadIk(VMDFile vmd, BinaryReader binaryReader)
    {
        uint ikCount = binaryReader.ReadUInt32();
        vmd.Iks.Resize(ikCount);

        for (int i = 0; i < ikCount; i++)
        {
            Ik ik = new()
            {
                Frame = binaryReader.ReadUInt32(),
                Show = binaryReader.ReadBoolean()
            };

            uint ikInfoCount = binaryReader.ReadUInt32();
            Array.Resize(ref ik.IkInfos, (int)ikInfoCount);

            for (int j = 0; j < ikInfoCount; j++)
            {
                IkInfo ikInfo = new()
                {
                    Name = binaryReader.ReadString(20, EncodingType.ShiftJIS),
                    Enable = binaryReader.ReadBoolean()
                };

                ik.IkInfos[j] = ikInfo;
            }

            vmd.Iks[i] = ik;
        }
    }
}
