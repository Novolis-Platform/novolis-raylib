namespace Novolis.Raylib.Shell;

/// <summary>Configuration for <see cref="RaylibEmbeddedShell"/> (hidden host window + framebuffer streaming).</summary>
public sealed class RaylibEmbeddedOptions
{
    /// <summary>Initial framebuffer width in pixels.</summary>
    public int Width { get; init; } = 640;

    /// <summary>Initial framebuffer height in pixels.</summary>
    public int Height { get; init; } = 480;

    /// <summary>Hidden GLFW window title (not shown to the user).</summary>
    public string WindowTitle { get; init; } = "Novolis.Raylib.Embedded";

    /// <summary>Target frames per second for the embedded loop.</summary>
    public int TargetFps { get; init; } = 60;

    /// <summary>When true, initializes the GLFW window with the hidden flag.</summary>
    public bool HideWindow { get; init; } = true;

    /// <summary>When true, disables the default Escape-to-close behavior.</summary>
    public bool DisableExitKey { get; init; } = true;
}
