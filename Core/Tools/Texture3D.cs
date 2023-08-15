﻿using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.OpenGLES.Extensions.EXT;

namespace Core.Tools;

public unsafe class Texture3D : IDisposable
{
    private readonly GL _gl;
    private readonly GLEnum _internalformat;
    private readonly uint _pbo;
    private readonly uint _tex;

    public Texture3D(GL gl, GLEnum internalformat, GLEnum wrapParam)
    {
        _gl = gl;
        _internalformat = internalformat;

        _pbo = _gl.GenBuffer();
        _tex = _gl.GenTexture();

        _gl.GetFloat((GLEnum)EXT.MaxTextureMaxAnisotropyExt, out float maxAnisotropy);

        _gl.BindTexture(GLEnum.TextureCubeMap, _tex);

        _gl.TexParameter(GLEnum.TextureCubeMap, (GLEnum)EXT.MaxTextureMaxAnisotropyExt, maxAnisotropy);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureWrapS, (int)wrapParam);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureWrapT, (int)wrapParam);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureWrapR, (int)wrapParam);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureBaseLevel, 0);
        _gl.TexParameter(GLEnum.TextureCubeMap, GLEnum.TextureMaxLevel, 8);

        _gl.BindTexture(GLEnum.TextureCubeMap, 0);
    }

    public void Enable()
    {
        _gl.BindTexture(GLEnum.TextureCubeMap, _tex);
    }

    public void Disable()
    {
        _gl.BindTexture(GLEnum.TextureCubeMap, 0);
    }

    public void AllocationBuffer(uint pboSize, out nint pboData, bool needRead = false)
    {
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, _pbo);

        _gl.BufferData(GLEnum.PixelUnpackBuffer, pboSize, null, GLEnum.StreamDraw);

        pboData = (nint)_gl.MapBufferRange(GLEnum.PixelUnpackBuffer, 0, pboSize, needRead ? (uint)(GLEnum.MapReadBit | GLEnum.MapWriteBit) : (uint)GLEnum.MapWriteBit);

        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void FlushTexture(Vector2D<uint> size, GLEnum target, GLEnum format, GLEnum type)
    {
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, _pbo);
        _gl.BindTexture(GLEnum.TextureCubeMap, _tex);

        _gl.UnmapBuffer(GLEnum.PixelUnpackBuffer);
        _gl.TexImage2D(target, 0, (int)_internalformat, size.X, size.Y, 0, format, type, null);
        _gl.GenerateMipmap(GLEnum.TextureCubeMap);

        _gl.BindTexture(GLEnum.TextureCubeMap, 0);
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(_pbo);
        _gl.DeleteTexture(_tex);

        GC.SuppressFinalize(this);
    }
}