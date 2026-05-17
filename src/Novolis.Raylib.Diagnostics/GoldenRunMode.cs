namespace Novolis.Raylib.Diagnostics;

/// <summary>How a golden test run validates and writes artifacts.</summary>
public enum GoldenRunMode
{
    Assert,
    UpdateBaselines,
    ReportOnly,
}
