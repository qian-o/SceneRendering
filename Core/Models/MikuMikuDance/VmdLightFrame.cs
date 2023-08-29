using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

public class VmdLightFrame
{
    public int Frame { get; }

    public Vector3D<float> Color { get; }

    public Vector3D<float> Position { get; }

    public VmdLightFrame(Stream stream)
    {
        byte[] buffer = new byte[sizeof(int)];
        stream.Read(buffer, 0, buffer.Length);
        Frame = BitConverter.ToInt32(buffer);

        buffer = new byte[sizeof(float) * 3];
        stream.Read(buffer, 0, buffer.Length);
        Color = new Vector3D<float>(BitConverter.ToSingle(buffer), BitConverter.ToSingle(buffer, sizeof(float)), BitConverter.ToSingle(buffer, sizeof(float) * 2));

        buffer = new byte[sizeof(float) * 3];
        stream.Read(buffer, 0, buffer.Length);
        Position = new Vector3D<float>(BitConverter.ToSingle(buffer), BitConverter.ToSingle(buffer, sizeof(float)), BitConverter.ToSingle(buffer, sizeof(float) * 2));
    }
}
