using System.Runtime.InteropServices;

namespace Novolis.Raylib.Interop;

/// <summary>Blittable RGBA layout for raylib <c>Color</c> (internal interop only).</summary>
[StructLayout(LayoutKind.Sequential)]
internal struct RaylibColor
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;
}
