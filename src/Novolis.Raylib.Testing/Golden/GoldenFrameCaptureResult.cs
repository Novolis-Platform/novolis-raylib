namespace Novolis.Raylib.Testing.Golden;

/// <summary>Capture and assert outcome for one golden frame.</summary>
public sealed class GoldenFrameCaptureResult
{
    public required GoldenFrameSpec Frame { get; init; }

    public byte[]? ActualPng { get; init; }

    public byte[]? BaselinePng { get; init; }

    public bool? AssertPassed { get; init; }

    public bool Skipped { get; init; }

    public string? SkipReason { get; init; }

    public string? ActualSha256 { get; init; }

    public string? BaselineSha256 { get; init; }

    public string? ErrorMessage { get; init; }
}
