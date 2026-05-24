namespace Novolis.Raylib.Testing.Golden;

/// <summary>Capture and assert outcome for one golden frame.</summary>
public sealed class GoldenFrameCaptureResult
{
    /// <summary>Frame specification that was captured.</summary>
    public required GoldenFrameSpec Frame { get; init; }

    /// <summary>Captured PNG bytes from this run.</summary>
    public byte[]? ActualPng { get; init; }

    /// <summary>Baseline PNG bytes used for comparison.</summary>
    public byte[]? BaselinePng { get; init; }

    /// <summary>True/false when assert ran; null in report-only mode.</summary>
    public bool? AssertPassed { get; init; }

    /// <summary>True when capture or assert was skipped for this frame.</summary>
    public bool Skipped { get; init; }

    /// <summary>Reason the frame was skipped.</summary>
    public string? SkipReason { get; init; }

    /// <summary>SHA-256 hex digest of <see cref="ActualPng"/>.</summary>
    public string? ActualSha256 { get; init; }

    /// <summary>SHA-256 hex digest of the baseline PNG or embedded hash.</summary>
    public string? BaselineSha256 { get; init; }

    /// <summary>Assert or harness error message for this frame.</summary>
    public string? ErrorMessage { get; init; }
}
