using Core.Helpers;

namespace Core.Models.MikuMikuDance;

public class VmdIkEnable
{
    public string Name { get; }

    public bool Enable { get; }

    public VmdIkEnable(Stream stream)
    {
        byte[] buffer = new byte[20];
        stream.Read(buffer, 0, buffer.Length);
        Name = buffer.Decode(EncodingType.ShiftJIS);

        buffer = new byte[1];
        stream.Read(buffer, 0, buffer.Length);
        Enable = buffer[0] == 1;
    }
}
