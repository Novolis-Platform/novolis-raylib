namespace Novolis.Raylib.Testing.Golden;

/// <summary>Aggregate assert outcome for a golden story run.</summary>
public sealed class GoldenStoryAssertInfo
{
    public bool? AssertPassed { get; init; }

    public bool Skipped { get; init; }

    public string? SkipReason { get; init; }

    public string? ErrorMessage { get; init; }
}
