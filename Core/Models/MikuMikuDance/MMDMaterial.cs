using Silk.NET.Maths;

namespace Core.Models.MikuMikuDance;

#region Enums
public enum SphereTextureMode
{
    None,
    Mul,
    Add
}
#endregion

public unsafe struct MMDMaterial
{
    public Vector3D<float> Diffuse;

    public float Alpha;

    public Vector3D<float> Specular;

    public float SpecularPower;

    public Vector3D<float> Ambient;

    public SphereTextureMode EdgeFlag;

    public float EdgeSize;

    public Vector4D<float> EdgeColor;

    public fixed char Texture[1024];

    public fixed char SpTexture[1024];

    public SphereTextureMode SpTextureMode;

    public fixed char ToonTexture[1024];

    public Vector4D<float> TextureMulFactor;

    public Vector4D<float> SpTextureMulFactor;

    public Vector4D<float> ToonTextureMulFactor;

    public Vector4D<float> TextureAddFactor;

    public Vector4D<float> SpTextureAddFactor;

    public Vector4D<float> ToonTextureAddFactor;

    public bool BothFace;

    public bool GroundShadow;

    public bool ShadowCaster;

    public bool ShadowReceiver;
}
