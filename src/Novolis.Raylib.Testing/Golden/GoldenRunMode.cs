namespace Novolis.Raylib.Testing.Golden;

/// <summary>How a golden test run validates and writes artifacts.</summary>
public enum GoldenRunMode
{
    /// <summary>Compare captures to committed baselines and fail on mismatch.</summary>
    Assert,

    /// <summary>Write new baseline PNGs and update embedded hashes in spec.</summary>
    UpdateBaselines,

    /// <summary>Capture and write QA reports without asserting.</summary>
    ReportOnly,
}
