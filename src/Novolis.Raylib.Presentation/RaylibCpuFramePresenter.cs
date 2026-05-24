using System.Runtime.InteropServices;
using Novolis.Math.Geometry;
using Novolis.Raylib.Bindings.Interop;
using Novolis.Raylib.Rendering;
using Novolis.Rendering.Presentation.Abstractions;

namespace Novolis.Raylib.Presentation;

/// <summary>Uploads CPU RGBA frames to a Raylib texture and draws full-screen.</summary>
public sealed class RaylibCpuFramePresenter : IFramePresenter, IDisposable
{
    private const int PixelFormatR8G8B8A8 = 7;
    private Texture _texture;
    private int _width;
    private int _height;
    private byte[]? _uploadBuffer;

    public void PresentCpuFrame(ReadOnlySpan<Rgba32> pixels, int width, int height)
    {
        EnsureTexture(width, height);
        CopyToBuffer(pixels);
        unsafe
        {
            fixed (byte* ptr = _uploadBuffer)
            {
                if (_texture.IsValid)
                {
                    RaylibPresentationNative.UpdateTexture(_texture.Native, (nint)ptr);
                }
            }
        }

        Textures.Draw(_texture, 0, 0, System.Drawing.Color.White);
    }

    public void Dispose()
    {
        if (_texture.IsValid)
        {
            Textures.Unload(_texture);
        }
    }

    private void EnsureTexture(int width, int height)
    {
        if (_texture.IsValid && _width == width && _height == height)
        {
            return;
        }

        if (_texture.IsValid)
        {
            Textures.Unload(_texture);
        }

        _width = width;
        _height = height;
        _uploadBuffer = new byte[width * height * 4];
        unsafe
        {
            fixed (byte* ptr = _uploadBuffer)
            {
                var image = new Raylib6NativeImage
                {
                    Data = (nint)ptr,
                    Width = width,
                    Height = height,
                    Mipmaps = 1,
                    Format = PixelFormatR8G8B8A8,
                };
                _texture = Texture.FromNative(RaylibPresentationNative.LoadTextureFromImage(image));
            }
        }
    }

    private void CopyToBuffer(ReadOnlySpan<Rgba32> pixels)
    {
        if (_uploadBuffer is null || _uploadBuffer.Length < pixels.Length * 4)
        {
            _uploadBuffer = new byte[pixels.Length * 4];
        }

        for (var i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            _uploadBuffer[i * 4] = p.R;
            _uploadBuffer[i * 4 + 1] = p.G;
            _uploadBuffer[i * 4 + 2] = p.B;
            _uploadBuffer[i * 4 + 3] = p.A;
        }
    }
}

internal static partial class RaylibPresentationNative
{
    private const string RaylibDll = "raylib";

    [LibraryImport(RaylibDll)]
    internal static partial Raylib6NativeTexture LoadTextureFromImage(Raylib6NativeImage image);

    [LibraryImport(RaylibDll)]
    internal static partial void UpdateTexture(Raylib6NativeTexture texture, nint pixels);
}
