#nullable enable

namespace Novolis.Raylib.Interop;

internal static unsafe partial class RayguiShimExports
{
    /// <summary>Raygui style reset; call once after <see cref="TryBindShim"/> succeeds.</summary>
    internal static void ApplyDefaultGuiStyle() => GuiLoadStyleDefault_ptr();
}
