namespace Novolis.Raylib.Shell;

/// <summary>One RGBA framebuffer snapshot from an embedded Raylib host (top-down, 4 bytes per pixel).</summary>
public readonly struct RaylibEmbeddedFrame
{
    /// <summary>Creates a frame descriptor.</summary>
    public RaylibEmbeddedFrame(ReadOnlyMemory<byte> rgbaPixels, int width, int height)
    {
        RgbaPixels = rgbaPixels;
        Width = width;
        Height = height;
    }

    /// <summary>Top-down RGBA bytes (length <c>width * height * 4</c>).</summary>
    public ReadOnlyMemory<byte> RgbaPixels { get; }

    /// <summary>Framebuffer width in pixels.</summary>
    public int Width { get; }

    /// <summary>Framebuffer height in pixels.</summary>
    public int Height { get; }
}
