namespace Core.Models;

public class Animation
{
    public string Name { get; }

    public double Duration { get; }

    public double TicksPerSecond { get; }

    public AnimationNode[] Channels { get; }

    public Animation(string name, double duration, double ticksPerSecond, AnimationNode[] channels)
    {
        Name = name;
        Duration = duration;
        TicksPerSecond = ticksPerSecond;
        Channels = channels;
    }
}
