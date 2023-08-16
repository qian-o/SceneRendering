﻿using Core.Tools;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using System.Drawing;

namespace Core.Helpers;

public static unsafe class TextureExtensions
{
    public static void WriteImage(this Texture2D texture, string file)
    {
        SKImage image = SKImage.FromEncodedData(file);

        texture.AllocationBuffer((uint)(image.Width * image.Height * 4), out void* pboData);

        image.ReadPixels(new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888), (nint)pboData, image.Width * 4, 0, 0);

        texture.FlushTexture(new Vector2D<uint>((uint)image.Width, (uint)image.Height), GLEnum.Rgba, GLEnum.UnsignedByte);

        image.Dispose();
    }

    public static void WriteImage(this Texture2D texture, byte* image, int width, int height)
    {
        texture.AllocationBuffer((uint)(width * height * 4), out void* pboData);

        Span<byte> src = new(image, width * height * 4);
        Span<byte> dst = new(pboData, width * height * 4);

        src.CopyTo(dst);

        texture.FlushTexture(new Vector2D<uint>((uint)width, (uint)height), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteColor(this Texture2D texture, Color color)
    {
        WriteColor(texture, new Vector4D<byte>(color.R, color.G, color.B, color.A));
    }

    public static void WriteColor(this Texture2D texture, Vector3D<byte> color)
    {
        WriteColor(texture, new Vector4D<byte>(color, 255));
    }

    public static void WriteColor(this Texture2D texture, Vector4D<byte> color)
    {
        texture.AllocationBuffer(4, out void* pboData);

        Span<Vector4D<byte>> span = new(pboData, 1);

        span[0] = color;

        texture.FlushTexture(new Vector2D<uint>(1, 1), GLEnum.Rgba, GLEnum.UnsignedByte);
    }

    public static void WriteImage(this Texture3D texture, GLEnum target, string file)
    {
        SKImage image = SKImage.FromEncodedData(file);

        texture.AllocationBuffer((uint)(image.Width * image.Height * 4), out nint pboData);

        image.ReadPixels(new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888), pboData, image.Width * 4, 0, 0);

        texture.FlushTexture(new Vector2D<uint>((uint)image.Width, (uint)image.Height), target, GLEnum.Rgba, GLEnum.UnsignedByte);

        image.Dispose();
    }

    public static void WriteColor(this Texture3D texture, GLEnum target, Color color)
    {
        WriteColor(texture, target, new Vector4D<byte>(color.R, color.G, color.B, color.A));
    }

    public static void WriteColor(this Texture3D texture, GLEnum target, Vector3D<byte> color)
    {
        WriteColor(texture, target, new Vector4D<byte>(color, 255));
    }

    public static void WriteColor(this Texture3D texture, GLEnum target, Vector4D<byte> color)
    {
        texture.AllocationBuffer(4, out nint pboData);

        Span<Vector4D<byte>> span = new((void*)pboData, 1);

        span[0] = color;

        texture.FlushTexture(new Vector2D<uint>(1, 1), target, GLEnum.Rgba, GLEnum.UnsignedByte);
    }
}
