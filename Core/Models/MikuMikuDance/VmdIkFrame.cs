namespace Core.Models.MikuMikuDance;

public class VmdIkFrame
{
    public int Frame { get; }

    public bool Display { get; }

    public List<VmdIkEnable> IkEnable { get; } = new List<VmdIkEnable>();

    public VmdIkFrame(Stream stream)
    {
        byte[] buffer = new byte[4];
        stream.Read(buffer, 0, buffer.Length);
        Frame = BitConverter.ToInt32(buffer, 0);

        buffer = new byte[1];
        stream.Read(buffer, 0, buffer.Length);
        Display = buffer[0] == 1;

        buffer = new byte[sizeof(int)];
        stream.Read(buffer, 0, buffer.Length);
        int ik_enable_num = BitConverter.ToInt32(buffer, 0);
        for (int i = 0; i < ik_enable_num; i++)
        {
            IkEnable.Add(new VmdIkEnable(stream));
        }
    }
}
