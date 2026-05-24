using System.Numerics;

namespace Novolis.Raylib.Rendering;

/// <summary>Scene camera (raylib <c>Camera3D</c> layout).</summary>
public struct Camera
{
    /// <summary>Camera position in world space.</summary>
    public Vector3 Position;

    /// <summary>Point the camera looks at.</summary>
    public Vector3 Target;

    /// <summary>Up direction (typically world +Y).</summary>
    public Vector3 Up;

    /// <summary>Field of view in degrees (perspective) or size in world units (orthographic).</summary>
    public float Fovy;

    /// <summary><see cref="CameraProjection.Perspective"/> or <see cref="CameraProjection.Orthographic"/>.</summary>
    public int Projection;

    /// <summary>Creates a perspective camera with raylib defaults.</summary>
    public static Camera Perspective(Vector3 position, Vector3 target, Vector3 up, float fovyDegrees = 60f) =>
        new()
        {
            Position = position,
            Target = target,
            Up = up,
            Fovy = fovyDegrees,
            Projection = CameraProjection.Perspective,
        };
}

/// <summary>raylib <c>CAMERA_PERSPECTIVE</c> / <c>CAMERA_ORTHOGRAPHIC</c>.</summary>
public static class CameraProjection
{
    /// <summary>Perspective projection (<c>CAMERA_PERSPECTIVE</c>).</summary>
    public const int Perspective = 0;

    /// <summary>Orthographic projection (<c>CAMERA_ORTHOGRAPHIC</c>).</summary>
    public const int Orthographic = 1;
}
