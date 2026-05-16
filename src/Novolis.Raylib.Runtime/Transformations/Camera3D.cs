using System.Runtime.InteropServices;

namespace Novolis.Raylib.Transformations;

/// <summary>3D camera (raylib <c>Camera3D</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Camera3D
{
    public Vector3 Position;
    public Vector3 Target;
    public Vector3 Up;
    public float Fovy;
    public int Projection;

    public static Camera3D Perspective(Vector3 position, Vector3 target, Vector3 up, float fovyDegrees = 60f) =>
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
    public const int Perspective = 0;
    public const int Orthographic = 1;
}
