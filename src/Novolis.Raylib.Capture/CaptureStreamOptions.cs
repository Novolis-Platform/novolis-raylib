namespace Novolis.Raylib.Capture;

/// <summary>Options for per-frame streaming capture via <see cref="FrameCaptureSession"/>.</summary>
public sealed class CaptureStreamOptions
{
    public int CaptureEveryNFrames { get; init; } = 1;

    public int MaxBufferedFrames { get; init; } = 32;
}
