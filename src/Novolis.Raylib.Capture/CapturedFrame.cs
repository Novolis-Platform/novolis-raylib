namespace Novolis.Raylib.Capture;

/// <summary>One framebuffer snapshot from streaming capture.</summary>
public sealed class CapturedFrame
{
    public required int FrameIndex { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required byte[] Png { get; init; }

    public required TimeSpan Elapsed { get; init; }
}
