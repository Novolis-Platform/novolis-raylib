using System.Runtime.InteropServices;

namespace Novolis.Raylib.Transformations;

/// <summary>Axis-aligned rectangle (raylib <c>Rectangle</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Rectangle
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public Rectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
