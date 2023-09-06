namespace Core.Models.MikuMikuDance;

public class MMDMorph
{
    public string Name { get; set; }

    public float Weight { get; set; }

    public float BaseAnimationWeight { get; private set; }

    protected MMDMorph()
    {
        Name = string.Empty;
        Weight = 0.0f;
        BaseAnimationWeight = 0.0f;
    }

    public void SaveBaseAnimation()
    {
        BaseAnimationWeight = Weight;
    }

    public void LoadBaseAnimation()
    {
        Weight = BaseAnimationWeight;
    }

    public void ClearBaseAnimation()
    {
        BaseAnimationWeight = 0.0f;
    }
}
