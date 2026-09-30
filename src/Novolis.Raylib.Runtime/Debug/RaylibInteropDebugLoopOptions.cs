namespace Novolis.Raylib.Debug;

/// <inheritdoc cref="RaylibDebug.LoopOptions"/>
public sealed class RaylibInteropDebugLoopOptions
{
    /// <inheritdoc cref="RaylibDebug.LoopOptions.Width"/>
    public int Width { get; init; } = 320;

    /// <inheritdoc cref="RaylibDebug.LoopOptions.Height"/>
    public int Height { get; init; } = 240;

    /// <inheritdoc cref="RaylibDebug.LoopOptions.WindowTitle"/>
    public string WindowTitle { get; init; } = "Novolis.Raylib.InteropDebug";

    /// <inheritdoc cref="RaylibDebug.LoopOptions.HideWindow"/>
    public bool HideWindow { get; init; }

    /// <inheritdoc cref="RaylibDebug.LoopOptions.MaxFrames"/>
    public int MaxFrames { get; init; } = 3;
}
