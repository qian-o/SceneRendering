using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;

namespace Core.Helpers;

public static unsafe class ImageHelper
{
    public static void ExportImage(byte* image, GLEnum format, int width, int height, string filePath)
    {
        using FileStream stream = File.Create(filePath);

        Span<Vector4D<byte>> span = new(image, width * height);
        span.Reverse();

        using SKBitmap bitmap = new(new SKImageInfo(width, height, format == GLEnum.Rgba ? SKColorType.Rgba8888 : SKColorType.Bgra8888));
        bitmap.InstallPixels(bitmap.Info, (nint)image, width * 4);

        bitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
    }
}
