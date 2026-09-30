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
