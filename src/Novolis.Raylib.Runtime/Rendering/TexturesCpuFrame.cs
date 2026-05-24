using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.Rendering;

public static partial class Textures
{
    private const int PixelFormatR8G8B8A8 = 7;

    /// <summary>Creates a GPU texture from contiguous RGBA8888 bytes.</summary>
    public static Texture LoadFromRgba(ReadOnlySpan<byte> rgba, int width, int height)
    {
        unsafe
        {
            fixed (byte* ptr = rgba)
            {
                var image = new Raylib6NativeImage
                {
                    Data = (nint)ptr,
                    Width = width,
                    Height = height,
                    Mipmaps = 1,
                    Format = PixelFormatR8G8B8A8,
                };
                return Texture.FromNative(RaylibCpuFrameNative.LoadTextureFromImage(image));
            }
        }
    }

    /// <summary>Uploads new RGBA8888 bytes into an existing texture.</summary>
    public static void UpdateRgba(Texture texture, ReadOnlySpan<byte> rgba)
    {
        unsafe
        {
            fixed (byte* ptr = rgba)
            {
                RaylibCpuFrameNative.UpdateTexture(texture.Native, (nint)ptr);
            }
        }
    }

    private static class RaylibCpuFrameNative
    {
        private const string RaylibDll = "raylib";

        [DllImport(RaylibDll, EntryPoint = "LoadTextureFromImage")]
        internal static extern Raylib6NativeTexture LoadTextureFromImage(Raylib6NativeImage image);

        [DllImport(RaylibDll, EntryPoint = "UpdateTexture")]
        internal static extern void UpdateTexture(Raylib6NativeTexture texture, nint pixels);
    }
}
