namespace Novolis.Raylib.Windowing;

/// <summary>Flags for <see cref="Window.SetConfigFlags"/> / <see cref="Window.SetState"/> (raylib <c>ConfigFlags</c>).</summary>
public static class WindowStateFlags
{
    /// <summary>Vertical sync (<c>FLAG_VSYNC_HINT</c>).</summary>
    public const uint VSync = 0x0000_0040;

    /// <summary>Resizable window (<c>FLAG_WINDOW_RESIZABLE</c>).</summary>
    public const uint Resizable = 0x0000_0004;

    /// <summary>4× MSAA (<c>FLAG_MSAA_4X_HINT</c>).</summary>
    public const uint Msaa4x = 0x0000_0020;

    /// <summary>High-DPI aware window (<c>FLAG_WINDOW_HIGHDPI</c>).</summary>
    public const uint HighDpi = 0x0000_2000;

    /// <summary>Matches raylib <c>FLAG_WINDOW_HIDDEN</c> (0x00000080).</summary>
    public const uint Hidden = 0x0000_0080;

    /// <summary>Default production game window: sharp scaling, resize, vsync.</summary>
    public const uint DefaultGameHost =
        VSync | Resizable | Msaa4x | HighDpi;
}
