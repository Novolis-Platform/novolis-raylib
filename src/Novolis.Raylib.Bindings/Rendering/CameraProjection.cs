using System.Numerics;

namespace Novolis.Raylib.Rendering;

/// <summary>raylib <c>CAMERA_PERSPECTIVE</c> / <c>CAMERA_ORTHOGRAPHIC</c>.</summary>
public static class CameraProjection
{
    /// <summary>Perspective projection (<c>CAMERA_PERSPECTIVE</c>).</summary>
    public const int Perspective = 0;

    /// <summary>Orthographic projection (<c>CAMERA_ORTHOGRAPHIC</c>).</summary>
    public const int Orthographic = 1;
}
