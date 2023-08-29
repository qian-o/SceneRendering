using Core.Helpers;
using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public class VmdBoneFrame
{
    public string Name { get; }

    public int Frame { get; }

    public Vector3D<float> Position { get; }

    public Quaternion<float> Rotation { get; }

    public byte[] Interpolation { get; } = new byte[64];

    public VmdBoneFrame(Stream stream)
    {
        byte[] buffer = new byte[15];
        stream.Read(buffer, 0, buffer.Length);
        Name = buffer.Decode(EncodingType.ShiftJIS);

        buffer = new byte[sizeof(int)];
        stream.Read(buffer, 0, buffer.Length);
        Frame = BitConverter.ToInt32(buffer);

        buffer = new byte[sizeof(float) * 3];
        stream.Read(buffer, 0, buffer.Length);
        Position = new Vector3D<float>(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8));

        buffer = new byte[sizeof(float) * 4];
        stream.Read(buffer, 0, buffer.Length);
        Rotation = new Quaternion<float>(BitConverter.ToSingle(buffer, 0), BitConverter.ToSingle(buffer, 4), BitConverter.ToSingle(buffer, 8), -BitConverter.ToSingle(buffer, 12));

        buffer = new byte[64];
        stream.Read(buffer, 0, buffer.Length);
        Interpolation = buffer;
    }
}
