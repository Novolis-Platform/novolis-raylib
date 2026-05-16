using System.Runtime.InteropServices;

namespace Novolis.Raylib.Transformations;

/// <summary>3D vector (raylib <c>Vector3</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;
}
