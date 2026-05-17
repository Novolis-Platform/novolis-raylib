using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Options for <see cref="RaylibGoldenTest.Run"/> (no environment variables).</summary>
public sealed class GoldenRunOptions
{
    public GoldenRunMode Mode { get; init; } = GoldenRunMode.Assert;

    public string? OutputRoot { get; init; }

    public IGoldenRunBucketLayout? RunBucketLayout { get; init; }

    public bool EnableStreamingCapture { get; init; }

    public int CaptureEveryNFrames { get; init; } = 1;

    public int MaxBufferedFrames { get; init; } = 32;

    public Assembly? TestAssembly { get; init; }

    /// <summary>Override committed <c>Goldens/</c> root (for baseline seed console).</summary>
    public string? GoldensRoot { get; init; }

    /// <summary>Optional second copy of the QA bundle via <see cref="GoldenArtifactPublisher"/>.</summary>
    public string? MirrorPublishDirectory { get; init; }

    public GoldenPublishOptions? MirrorPublishOptions { get; init; }
}
