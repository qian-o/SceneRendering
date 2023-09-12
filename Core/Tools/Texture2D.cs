using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;

namespace Core.Tools;

public unsafe class Texture2D : IDisposable
{
    private readonly GL _gl;
    private readonly GLEnum _internalformat;
    private readonly uint _tex;

    public Texture2D(GL gl, GLEnum internalformat, GLEnum wrapParam)
    {
        _gl = gl;
        _internalformat = internalformat;

        _tex = _gl.GenTexture();

        _gl.GetFloat((GLEnum)EXT.MaxTextureMaxAnisotropyExt, out float maxAnisotropy);

        _gl.BindTexture(GLEnum.Texture2D, _tex);

        _gl.TexParameter(GLEnum.Texture2D, (GLEnum)EXT.MaxTextureMaxAnisotropyExt, maxAnisotropy);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)wrapParam);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)wrapParam);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureBaseLevel, 0);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMaxLevel, 8);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Enable()
    {
        _gl.BindTexture(GLEnum.Texture2D, _tex);
    }

    public void Disable()
    {
        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void FlushTexture(void* image, Vector2D<uint> size, GLEnum format, GLEnum type)
    {
        _gl.BindTexture(GLEnum.Texture2D, _tex);

        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)_internalformat, size.X, size.Y, 0, format, type, image);
        _gl.GenerateMipmap(GLEnum.Texture2D);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_tex);

        GC.SuppressFinalize(this);
    }
}
