using Core.Helpers;

namespace Core.Models.MikuMikuDance;

public class VmdFaceFrame
{
    public string Name { get; }

    public int Frame { get; }

    public float Weight { get; }

    public VmdFaceFrame(Stream stream)
    {
        byte[] buffer = new byte[15];
        stream.Read(buffer, 0, buffer.Length);
        Name = buffer.Decode(EncodingType.ShiftJIS);

        buffer = new byte[sizeof(int)];
        stream.Read(buffer, 0, buffer.Length);
        Frame = BitConverter.ToInt32(buffer);

        buffer = new byte[sizeof(float)];
        stream.Read(buffer, 0, buffer.Length);
        Weight = BitConverter.ToSingle(buffer);
    }
}
