using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Rendering;

/// <summary>GPU texture handle (wraps manifest-generated <see cref="Raylib6NativeTexture"/>).</summary>
public readonly struct Texture
{
    internal Raylib6NativeTexture Native { get; init; }

    /// <summary>OpenGL texture id (0 when unloaded).</summary>
    public uint Id => Native.Id;

    /// <summary>Texture width in pixels.</summary>
    public int Width => Native.Width;

    /// <summary>Texture height in pixels.</summary>
    public int Height => Native.Height;

    /// <summary>True when the native handle is non-zero.</summary>
    public bool IsValid => Native.Id != 0;

    internal static Texture FromNative(Raylib6NativeTexture native) => new() { Native = native };
}
