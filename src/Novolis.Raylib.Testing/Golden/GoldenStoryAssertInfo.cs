namespace Novolis.Raylib.Testing.Golden;

/// <summary>Aggregate assert outcome for a golden story run.</summary>
public sealed class GoldenStoryAssertInfo
{
    /// <summary>True/false when assert ran; null in report-only mode.</summary>
    public bool? AssertPassed { get; init; }

    /// <summary>True when the entire story run was skipped.</summary>
    public bool Skipped { get; init; }

    /// <summary>Reason the story run was skipped.</summary>
    public string? SkipReason { get; init; }

    /// <summary>Combined assert or harness error message.</summary>
    public string? ErrorMessage { get; init; }
}
