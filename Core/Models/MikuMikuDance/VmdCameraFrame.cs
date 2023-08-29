using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public class VmdCameraFrame
{
    public int Frame { get; }

    public float Distance { get; }

    public Vector3D<float> Position { get;  }

    public Vector3D<float> Orientation { get; }

    public byte[] Interpolation { get; } = new byte[24];

    public float ViewAngle { get; }

    public byte[] Unknown { get; } = new byte[4];

    public VmdCameraFrame(Stream stream)
    {
        byte[] buffer = new byte[sizeof(int)];
        stream.Read(buffer, 0, buffer.Length);
        Frame = BitConverter.ToInt32(buffer);

        buffer = new byte[sizeof(float)];
        stream.Read(buffer, 0, buffer.Length);
        Distance = BitConverter.ToSingle(buffer);

        buffer = new byte[sizeof(float) * 3];
        stream.Read(buffer, 0, buffer.Length);
        Position = new Vector3D<float>(BitConverter.ToSingle(buffer), BitConverter.ToSingle(buffer, sizeof(float)), BitConverter.ToSingle(buffer, sizeof(float) * 2));

        buffer = new byte[sizeof(float) * 3];
        stream.Read(buffer, 0, buffer.Length);
        Orientation = new Vector3D<float>(BitConverter.ToSingle(buffer), BitConverter.ToSingle(buffer, sizeof(float)), BitConverter.ToSingle(buffer, sizeof(float) * 2));

        buffer = new byte[24];
        stream.Read(buffer, 0, buffer.Length);
        Interpolation = buffer;

        buffer = new byte[sizeof(float)];
        stream.Read(buffer, 0, buffer.Length);
        ViewAngle = BitConverter.ToSingle(buffer);

        buffer = new byte[3];
        stream.Read(buffer, 0, buffer.Length);
        Unknown = buffer;
    }
}
