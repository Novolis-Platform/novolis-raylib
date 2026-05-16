using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Rendering;

/// <summary>GPU texture handle (wraps manifest-generated <see cref="Raylib6NativeTexture"/>).</summary>
public readonly struct Texture
{
    internal Raylib6NativeTexture Native { get; init; }

    public uint Id => Native.Id;

    public int Width => Native.Width;

    public int Height => Native.Height;

    public bool IsValid => Native.Id != 0;

    internal static Texture FromNative(Raylib6NativeTexture native) => new() { Native = native };
}
