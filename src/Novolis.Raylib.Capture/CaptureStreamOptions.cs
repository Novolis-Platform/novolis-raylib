namespace Novolis.Raylib.Capture;

/// <summary>Options for per-frame streaming capture via <see cref="FrameCaptureSession"/>.</summary>
public sealed class CaptureStreamOptions
{
    /// <summary>Capture every Nth presented frame (minimum 1).</summary>
    public int CaptureEveryNFrames { get; init; } = 1;

    /// <summary>Maximum frames buffered before oldest frames are dropped.</summary>
    public int MaxBufferedFrames { get; init; } = 32;
}
