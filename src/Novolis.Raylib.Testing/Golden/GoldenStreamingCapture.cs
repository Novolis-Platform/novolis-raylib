using Novolis.Raylib.Capture;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Test-only wrapper: streams frames during a golden run and writes PNGs on dispose.</summary>
public sealed class GoldenStreamingCapture : IDisposable
{
    private readonly FrameCaptureSession _session;
    private readonly string _outputDirectory;
    private bool _disposed;

    private GoldenStreamingCapture(string outputDirectory, GoldenRunOptions options)
    {
        _outputDirectory = outputDirectory;
        _session = new FrameCaptureSession(new CaptureStreamOptions
        {
            CaptureEveryNFrames = options.CaptureEveryNFrames,
            MaxBufferedFrames = options.MaxBufferedFrames,
        });
    }

    /// <summary>Starts streaming capture when enabled in options.</summary>
    /// <param name="storyDirectory">Directory for streamed PNG files.</param>
    /// <param name="options">Run options controlling streaming.</param>
    /// <returns>Capture instance, or null when streaming is disabled.</returns>
    public static GoldenStreamingCapture? TryStart(string storyDirectory, GoldenRunOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storyDirectory);
        ArgumentNullException.ThrowIfNull(options);
        if (!options.EnableStreamingCapture)
            return null;

        Directory.CreateDirectory(storyDirectory);
        return new GoldenStreamingCapture(storyDirectory, options);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        DrainCapturedFrames();
        _session.Dispose();
    }

    private void DrainCapturedFrames()
    {
        var reader = _session.Reader;
        if (reader is null)
            return;

        while (reader.TryRead(out var frame))
        {
            var path = Path.Combine(_outputDirectory, $"stream_{frame.FrameIndex:D4}.png");
            File.WriteAllBytes(path, frame.Png);
        }
    }
}
