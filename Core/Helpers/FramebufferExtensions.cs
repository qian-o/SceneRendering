using Silk.NET.OpenGLES;
using System.Runtime.InteropServices;

namespace Core.Helpers;

public static unsafe class FramebufferExtensions
{
    public static byte* GetFrameBufferImage(this GL gl, int width, int height)
    {
        byte* image = (byte*)Marshal.AllocHGlobal(width * height * 4);
        gl.ReadPixels(0, 0, (uint)width, (uint)height, GLEnum.Rgba, GLEnum.UnsignedByte, image);

        return image;
    }

    public static void ExportFrameBufferImage(this GL gl, int width, int height, string filePath)
    {
        byte* image = gl.GetFrameBufferImage(width, height);

        ImageHelper.ExportImage(image, GLEnum.Rgba, width, height, filePath);

        Marshal.FreeHGlobal((IntPtr)image);
    }
}
