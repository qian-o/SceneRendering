namespace Core.Contracts.Models.MikuMikuDance;

public abstract class MMDMorph
{
    private float saveAnimWeight;

    public string Name { get; set; }

    public float Weight { get; set; }

    public float BaseAnimationWeight => saveAnimWeight;

    protected MMDMorph()
    {
        Name = string.Empty;
        Weight = 0.0f;
        saveAnimWeight = 0.0f;
    }

    public void SaveBaseAnimation()
    {
        saveAnimWeight = Weight;
    }

    public void LoadBaseAnimation()
    {
        Weight = saveAnimWeight;
    }

    public void ClearBaseAnimation()
    {
        saveAnimWeight = 0.0f;
    }
}
