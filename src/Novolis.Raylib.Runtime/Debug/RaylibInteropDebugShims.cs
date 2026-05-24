namespace Novolis.Raylib.Debug;

/// <summary>Compatibility shims for callers that still use the former Debug assembly type names.</summary>
public static class RaylibInteropDebugRuntime
{
    /// <summary>Runs a minimal frame loop via <see cref="RaylibDebug.RunMinimalFrameLoop"/>.</summary>
    /// <param name="options">Loop options; defaults when <see langword="null"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Interop-named loop result.</returns>
    public static RaylibInteropDebugLoopResult RunMinimalFrameLoop(
        RaylibInteropDebugLoopOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var r = RaylibDebug.RunMinimalFrameLoop(
            options is null
                ? null
                : new RaylibDebug.LoopOptions
                {
                    Width = options.Width,
                    Height = options.Height,
                    WindowTitle = options.WindowTitle,
                    HideWindow = options.HideWindow,
                    MaxFrames = options.MaxFrames,
                },
            cancellationToken);
        return new RaylibInteropDebugLoopResult(r.Ok, r.Width, r.Height, r.FramesPresented);
    }
}

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

/// <inheritdoc cref="RaylibDebug.LoopResult"/>
/// <param name="Ok">Whether the window initialized successfully.</param>
/// <param name="Width">Window width used.</param>
/// <param name="Height">Window height used.</param>
/// <param name="FramesPresented">Number of frames presented.</param>
public readonly record struct RaylibInteropDebugLoopResult(bool Ok, int Width, int Height, int FramesPresented);
