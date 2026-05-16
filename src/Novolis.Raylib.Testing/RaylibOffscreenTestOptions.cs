namespace Novolis.Raylib.Testing;

/// <summary>Options for <see cref="RaylibOffscreenTestHarness.Run"/>.</summary>
public sealed class RaylibOffscreenTestOptions
{
    public string WindowTitle { get; init; } = "Novolis.Raylib.OffscreenTest";

    public int Width { get; init; } = 128;

    public int Height { get; init; } = 128;

    public int MaxFrames { get; init; } = 4;

    public bool HideWindow { get; init; } = true;

    public bool UsePostInitHideOnly { get; init; }

    public bool CaptureLastFramePng { get; init; }
}
