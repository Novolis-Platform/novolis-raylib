using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Rendering;

/// <summary>Exports the default framebuffer as PNG using raylib <c>LoadImageFromScreen</c> / <c>ExportImageToMemory</c>. Call from the render thread after a presented frame (e.g. after <see cref="Graphics.EndDrawing"/>).</summary>
public static class ScreenFramebufferCapture
{
    /// <summary>Captures the current screen buffer and returns PNG bytes. Returns <c>false</c> if export fails or produces no data.</summary>
    public static bool TryExportFramebufferToPng([NotNullWhen(true)] out byte[]? png)
    {
        png = null;
        var image = Raylib6Native.LoadImageFromScreen();
        try
        {
            if (image.Width <= 0 || image.Height <= 0)
                return false;

            // raylib strcmp(fileType, ".png") — extension must include the dot or export returns NULL (rtextures.c).
            var ptr = Raylib6Native.ExportImageToMemory(image, ".png", out var size);
            if (ptr == 0 || size <= 0)
                return false;
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
}
