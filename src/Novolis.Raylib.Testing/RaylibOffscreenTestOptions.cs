namespace Novolis.Raylib.Testing;

/// <summary>Options for <see cref="RaylibOffscreenTestHarness.Run"/>.</summary>
public sealed class RaylibOffscreenTestOptions
{
    /// <summary>Raylib window title during the test run.</summary>
    public string WindowTitle { get; init; } = "Novolis.Raylib.OffscreenTest";

    /// <summary>Framebuffer width in pixels.</summary>
    public int Width { get; init; } = 128;

    /// <summary>Framebuffer height in pixels.</summary>
    public int Height { get; init; } = 128;

    /// <summary>Maximum frames to render before stopping.</summary>
    public int MaxFrames { get; init; } = 4;

    /// <summary>When true, hides the window during the run.</summary>
    public bool HideWindow { get; init; } = true;

    /// <summary>When true with <see cref="HideWindow"/>, hides only after window init.</summary>
    public bool UsePostInitHideOnly { get; init; }

    /// <summary>When true, captures PNG after the final frame.</summary>
    public bool CaptureLastFramePng { get; init; }

    /// <summary>1-based frame numbers to capture as PNG after <c>EndDrawing</c> (empty = none unless <see cref="CaptureLastFramePng"/>).</summary>
    public IReadOnlyList<int> CaptureAtFrameNumbers { get; init; } = [];
}
