using System.Diagnostics;
using System.Threading.Channels;
using Novolis.Raylib.Presentation;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.Capture;

/// <summary>Streams framebuffer PNGs when a <see cref="FrameCaptureSession"/> is active.</summary>
public static class FrameCapturePipeline
{
    private static Channel<CapturedFrame>? _channel;
    private static int _frameIndex;
    private static readonly Stopwatch Stopwatch = new();
    private static CaptureStreamOptions? _options;

    public static ChannelReader<CapturedFrame>? Reader => _channel?.Reader;

    public static void Start(CaptureStreamOptions options)
    {
        Stop();
        _options = options;
        _frameIndex = 0;
        Stopwatch.Restart();
        _channel = Channel.CreateBounded<CapturedFrame>(new BoundedChannelOptions(options.MaxBufferedFrames)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        });

        RaylibPresentationHooks.Register(OnFramePresented, enabled: true);
    }

    public static void Stop()
    {
        RaylibPresentationHooks.Register(null, enabled: false);
        _channel = null;
        _options = null;
        _frameIndex = 0;
        Stopwatch.Reset();
    }

    private static void OnFramePresented()
    {
        var options = _options;
        var channel = _channel;
        if (options is null || channel is null || !RaylibCaptureRuntimeState.IsStreamingActive)
            return;

        _frameIndex++;
        if (_frameIndex % Math.Max(1, options.CaptureEveryNFrames) != 0)
            return;

        if (!ScreenFramebufferCapture.TryExportFramebufferToPng(out var png))
            return;

        var sw = Novolis.Raylib.Windowing.Window.GetScreenWidth();
        var sh = Novolis.Raylib.Windowing.Window.GetScreenHeight();
        var frame = new CapturedFrame
        {
            FrameIndex = _frameIndex,
            Width = sw,
            Height = sh,
            Png = png,
            Elapsed = Stopwatch.Elapsed,
        };

        channel.Writer.TryWrite(frame);
    }
}
