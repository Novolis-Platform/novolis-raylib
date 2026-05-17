namespace Novolis.Raylib.Capture;

/// <summary>Scoped framebuffer streaming state (no environment variables).</summary>
public static class RaylibCaptureRuntimeState
{
    private static readonly AsyncLocal<CaptureStreamOptions?> Current = new();

    public static bool IsStreamingActive => Current.Value is not null;

    public static CaptureStreamOptions? CurrentOptions => Current.Value;

    public static CaptureStreamScope Enter(CaptureStreamOptions options)
    {
        var previous = Current.Value;
        Current.Value = options;
        return new CaptureStreamScope(previous);
    }

    public sealed class CaptureStreamScope(CaptureStreamOptions? previous) : IDisposable
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
