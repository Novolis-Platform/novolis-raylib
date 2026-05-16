namespace Novolis.Raylib.Debug;

/// <summary>Compatibility shims for callers that still use the former Debug assembly type names.</summary>
public static class RaylibInteropDebugRuntime
{
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
    public int Width { get; init; } = 320;

    public int Height { get; init; } = 240;

    public string WindowTitle { get; init; } = "Novolis.Raylib.InteropDebug";

    public bool HideWindow { get; init; }

    public int MaxFrames { get; init; } = 3;
}

public readonly record struct RaylibInteropDebugLoopResult(bool Ok, int Width, int Height, int FramesPresented);
