using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Options for <see cref="RaylibGoldenTest.Run(string, Novolis.Raylib.Abstractions.IRaylibFrameRenderer, GoldenRunOptions?)"/> (no environment variables).</summary>
public sealed class GoldenRunOptions
{
    /// <summary>How captures are validated and written.</summary>
    public GoldenRunMode Mode { get; init; } = GoldenRunMode.Assert;

    /// <summary>Optional absolute root for adhoc render output.</summary>
    public string? OutputRoot { get; init; }

    /// <summary>Layout strategy for run and story directories.</summary>
    public IGoldenRunBucketLayout? RunBucketLayout { get; init; }

    /// <summary>When true, streams intermediate frames to disk during the run.</summary>
    public bool EnableStreamingCapture { get; init; }

    /// <summary>Capture every N frames when streaming is enabled.</summary>
    public int CaptureEveryNFrames { get; init; } = 1;

    /// <summary>Max in-memory buffered frames for streaming capture.</summary>
    public int MaxBufferedFrames { get; init; } = 32;

    /// <summary>Test assembly for goldens root and output layout (defaults to caller).</summary>
    public Assembly? TestAssembly { get; init; }

    /// <summary>Override committed <c>Goldens/</c> root (for baseline seed console).</summary>
    public string? GoldensRoot { get; init; }

    /// <summary>Optional second copy of the QA bundle via <see cref="GoldenArtifactPublisher"/>.</summary>
    public string? MirrorPublishDirectory { get; init; }

    /// <summary>Options for mirror publish when <see cref="MirrorPublishDirectory"/> is set.</summary>
    public GoldenPublishOptions? MirrorPublishOptions { get; init; }
}
