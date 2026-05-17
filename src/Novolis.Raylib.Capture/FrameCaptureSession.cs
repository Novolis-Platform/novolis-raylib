namespace Novolis.Raylib.Capture;

/// <summary>Scoped streaming framebuffer capture to a bounded channel.</summary>
public sealed class FrameCaptureSession : IDisposable
{
    private readonly RaylibCaptureRuntimeState.CaptureStreamScope _scope;
    private bool _disposed;

    public FrameCaptureSession(CaptureStreamOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _scope = RaylibCaptureRuntimeState.Enter(options);
        FrameCapturePipeline.Start(options);
    }

    public System.Threading.Channels.ChannelReader<CapturedFrame>? Reader => FrameCapturePipeline.Reader;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        FrameCapturePipeline.Stop();
        _scope.Dispose();
    }
}
