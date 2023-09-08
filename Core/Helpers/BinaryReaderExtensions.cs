﻿using Silk.NET.Maths;

namespace Core.Helpers;

public static unsafe class BinaryReaderExtensions
{
    public static string ReadString(this BinaryReader binaryReader, EncodingType encodingType = EncodingType.UTF8)
    {
        return binaryReader.ReadString(binaryReader.ReadUInt32(), encodingType);
    }

    public static string ReadString(this BinaryReader binaryReader, uint length, EncodingType encodingType = EncodingType.UTF8)
    {
        byte[] buffer = new byte[length];
        binaryReader.Read(buffer, 0, (int)length);

        return buffer.ToString(encodingType);
    }

    public static int ReadIndex(this BinaryReader binaryReader, byte indexSize)
    {
        switch (indexSize)
        {
            case 1:
                {
                    byte index = binaryReader.ReadByte();
                    if (index == byte.MaxValue)
                    {
                        return -1;
                    }

                    return index;
                }
            case 2:
                {
                    ushort index = binaryReader.ReadUInt16();
                    if (index == ushort.MaxValue)
                    {
                        return -1;
                    }

                    return index;
                }
            case 4:
                {
                    uint index = binaryReader.ReadUInt32();
                    if (index == uint.MaxValue)
                    {
                        return -1;
                    }

                    return (int)index;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(indexSize), indexSize, null);
        }
    }

    public static ushort[] ReadUInt16s(this BinaryReader binaryReader, int count)
    {
        ushort[] array = new ushort[count];

        for (int i = 0; i < count; i++)
        {
            array[i] = binaryReader.ReadUInt16();
        }

        return array;
    }

    public static uint[] ReadUInt32s(this BinaryReader binaryReader, int count)
    {
        uint[] array = new uint[count];

        for (int i = 0; i < count; i++)
        {
            array[i] = binaryReader.ReadUInt32();
        }

        return array;
    }

    public static Vector2D<float> ReadVector2D(this BinaryReader binaryReader)
    {
        return new(binaryReader.ReadSingle(), binaryReader.ReadSingle());
    }

    public static Vector3D<float> ReadVector3D(this BinaryReader binaryReader)
    {
        return new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
    }

    public static Vector4D<float> ReadVector4D(this BinaryReader binaryReader)
    {
        return new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
    }

    public static Quaternion<float> ReadQuaternion(this BinaryReader binaryReader)
    {
        return new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
    }
}
