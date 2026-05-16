using System.Drawing;

namespace Novolis.Raylib.Interop;

internal static class RaylibInteropMarshaling
{
    internal static RaylibColor ToRaylibColor(this Color color) =>
        new()
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A,
        };
}
