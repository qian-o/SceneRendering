namespace Core.Helpers;
public static class ShaderHelper
{
    // mvp.vert
    public const string PositionAttrib = "position";
    public const string NormalAttrib = "normal";
    public const string TexCoordsAttrib = "texCoords";

    public const string ModelUniform = "model";
    public const string ViewUniform = "view";
    public const string ProjectionUniform = "projection";

    // texture.frag
    public const string TextureUniform = "tex";

    // lighting.frag
    public const string ViewPosUniform = "viewPos";
    public const string MaterialUniform = "material";
    public const string DirLightUniform = "dirLight";
    public const string PointLightArrayUniform = "pointLights";
    public const string SpotLightUniform = "spotLight";
}
