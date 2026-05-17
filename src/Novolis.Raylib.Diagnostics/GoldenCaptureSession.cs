namespace Novolis.Raylib.Diagnostics;

/// <summary>Scoped streaming golden capture to a bounded channel.</summary>
public sealed class GoldenCaptureSession : IDisposable
{
    private bool _disposed;

    public GoldenCaptureSession(GoldenRuntimeFrame options)
    {
        if (options.EnableStreamingCapture)
            GoldenCaptureService.Start(options);
    }

    public System.Threading.Channels.ChannelReader<CapturedFrame>? Reader => GoldenCaptureService.Reader;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        GoldenCaptureService.Stop();
    }
}
