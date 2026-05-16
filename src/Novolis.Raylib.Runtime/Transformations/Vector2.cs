using System.Runtime.InteropServices;

namespace Novolis.Raylib.Transformations;

/// <summary>2D vector (raylib <c>Vector2</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector2
{
    public float X;
    public float Y;
}
