using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Rendering;

/// <summary>Exports the default framebuffer using raylib <c>LoadImageFromScreen</c>. Call from the render thread after a presented frame (e.g. after <see cref="Graphics.EndDrawing"/>).</summary>
public static class ScreenFramebufferCapture
{
    private const int PixelFormatR8G8B8A8 = 7;
    private const int BytesPerRgbaPixel = 4;

    /// <summary>
    /// Copies the current screen buffer into <paramref name="rgba"/> as top-down RGBA bytes.
    /// Call from the render thread after <see cref="Graphics.EndDrawing"/>.
    /// </summary>
    public static bool TryCopyFramebufferToRgba(Span<byte> rgba, int width, int height)
    {
        if (width <= 0 || height <= 0 || rgba.Length < width * height * BytesPerRgbaPixel)
        {
            return false;
        }

        var image = Raylib6Native.LoadImageFromScreen();
        try
        {
            if (image.Width <= 0 || image.Height <= 0 || image.Data == 0)
            {
                return false;
            }

            var srcW = image.Width;
            var srcH = image.Height;
            var format = image.Format;
            var bytesPerPixel = GetBytesPerPixel(format);
            unsafe
            {
                var src = new ReadOnlySpan<byte>((void*)image.Data, srcW * srcH * bytesPerPixel);
                for (var y = 0; y < height; y++)
                {
                    var srcY = MapSourceScanlineY(y, height, srcH);
                    var dstRow = y * width * BytesPerRgbaPixel;
                    for (var x = 0; x < width; x++)
                    {
                        var srcX = MapSourceColumnX(x, width, srcW);
                        WriteRgbaPixel(rgba, dstRow + x * BytesPerRgbaPixel, src, srcW, srcH, bytesPerPixel, srcX, srcY);
                    }
                }
            }

            return true;
        }
        finally
        {
            Raylib6Native.UnloadImage(image);
        }
    }

    /// <summary>Captures the current screen buffer and returns PNG bytes. Returns <c>false</c> if export fails or produces no data.</summary>
    public static bool TryExportFramebufferToPng([NotNullWhen(true)] out byte[]? png)
    {
        png = null;
        var image = Raylib6Native.LoadImageFromScreen();
        try
        {
            if (image.Width <= 0 || image.Height <= 0)
            {
                return false;
            }

            // raylib strcmp(fileType, ".png") — extension must include the dot or export returns NULL (rtextures.c).
            var ptr = Raylib6Native.ExportImageToMemory(image, ".png", out var size);
            if (ptr == 0 || size <= 0)
            {
                return false;
            }

            png = new byte[size];
            Marshal.Copy(ptr, png, 0, size);
            Raylib6Native.MemFree(ptr);
            return true;
        }
        finally
        {
            Raylib6Native.UnloadImage(image);
        }
    }

    private static int MapSourceScanlineY(int dstY, int dstH, int srcH) =>
        (int)((long)dstY * srcH / System.Math.Max(1, dstH));

    private static int MapSourceColumnX(int dstX, int dstW, int srcW) =>
        (int)((long)dstX * srcW / System.Math.Max(1, dstW));

    private static int GetBytesPerPixel(int format) =>
        format switch
        {
            PixelFormatR8G8B8A8 => BytesPerRgbaPixel,
            6 => 3, // PIXELFORMAT_UNCOMPRESSED_R8G8B8
            _ => BytesPerRgbaPixel,
        };

    private static void WriteRgbaPixel(
        Span<byte> dst,
        int dstIndex,
        ReadOnlySpan<byte> src,
        int srcW,
        int srcH,
        int bytesPerPixel,
        int x,
        int y)
    {
        // Raylib Image scanlines are top-down; do not flip or Avalonia hosts render upside-down.
        var index = (y * srcW + x) * bytesPerPixel;
        if ((uint)index + (uint)(bytesPerPixel - 1) >= (uint)src.Length
            || (uint)dstIndex + 3 >= (uint)dst.Length)
        {
            return;
        }

        dst[dstIndex] = src[index];
        dst[dstIndex + 1] = src[index + 1];
        dst[dstIndex + 2] = src[index + 2];
        dst[dstIndex + 3] = bytesPerPixel >= 4 ? src[index + 3] : (byte)255;
    }
}
