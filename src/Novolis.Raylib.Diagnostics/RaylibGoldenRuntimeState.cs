namespace Novolis.Raylib.Diagnostics;

/// <summary>Scoped golden capture and run mode (no environment variables).</summary>
public static class RaylibGoldenRuntimeState
{
    private static readonly AsyncLocal<GoldenRuntimeFrame?> Current = new();

    public static bool IsCaptureActive => Current.Value?.EnableStreamingCapture == true;

    public static GoldenRunMode Mode => Current.Value?.Mode ?? GoldenRunMode.Assert;

    public static GoldenRuntimeFrame? CurrentFrame => Current.Value;

    public static GoldenRuntimeScope Enter(GoldenRuntimeFrame frame)
    {
        var previous = Current.Value;
        Current.Value = frame;
        return new GoldenRuntimeScope(previous);
    }

    public sealed class GoldenRuntimeScope(GoldenRuntimeFrame? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Current.Value = previous;
        }
    }
}

/// <summary>Per-run golden state pushed onto <see cref="RaylibGoldenRuntimeState"/>.</summary>
public sealed class GoldenRuntimeFrame
{
    public required GoldenRunMode Mode { get; init; }

    public required string StoryDirectory { get; init; }

    public bool EnableStreamingCapture { get; init; }

    public int CaptureEveryNFrames { get; init; } = 1;

    public int MaxBufferedFrames { get; init; } = 32;
}
