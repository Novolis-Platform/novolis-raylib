using System.Runtime.InteropServices;

namespace Novolis.Raylib.Colors;

/// <summary>RGBA color (matches raylib <c>Color</c> layout).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Color : IEquatable<Color>
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public readonly bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    public override readonly bool Equals(object? obj) => obj is Color c && Equals(c);

    public override readonly int GetHashCode() => HashCode.Combine(R, G, B, A);

    public static bool operator ==(Color left, Color right) => left.Equals(right);

    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public static Color RayWhite => new(245, 245, 245, 255);

    public static Color DarkGray => new(80, 80, 80, 255);
}
