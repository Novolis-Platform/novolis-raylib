namespace Novolis.Raylib.Capture;

/// <summary>Scoped framebuffer streaming state (no environment variables).</summary>
public static class RaylibCaptureRuntimeState
{
    private static readonly AsyncLocal<CaptureStreamOptions?> Current = new();

    /// <summary>Whether a <see cref="FrameCaptureSession"/> is active on this async flow.</summary>
    public static bool IsStreamingActive => Current.Value is not null;

    /// <summary>Active capture options, or <see langword="null"/> when not streaming.</summary>
    public static CaptureStreamOptions? CurrentOptions => Current.Value;

    /// <summary>Enters streaming scope with the given options.</summary>
    /// <param name="options">Capture cadence and buffer size.</param>
    /// <returns>Disposable scope that restores the previous options.</returns>
    public static CaptureStreamScope Enter(CaptureStreamOptions options)
    {
        var previous = Current.Value;
        Current.Value = options;
        return new CaptureStreamScope(previous);
    }

    /// <summary>Restores the previous capture options when disposed.</summary>
    /// <param name="previous">Options to restore.</param>
    public sealed class CaptureStreamScope(CaptureStreamOptions? previous) : IDisposable
    {
        private bool _disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Current.Value = previous;
        }
    }
}
