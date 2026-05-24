using System.Drawing;

namespace Novolis.Raylib.Colors;

/// <summary>Common raylib palette colors (<see cref="Color"/> presets).</summary>
public static class RaylibColors
{
    /// <summary>Off-white preset matching raylib <c>RAYWHITE</c>.</summary>
    public static Color RayWhite => Color.FromArgb(255, 245, 245, 245);

    /// <summary>Dark gray preset matching raylib <c>DARKGRAY</c>.</summary>
    public static Color DarkGray => Color.FromArgb(255, 80, 80, 80);

    /// <summary>Opaque white.</summary>
    public static Color White => Color.White;

    /// <summary>Opaque black.</summary>
    public static Color Black => Color.Black;
}
