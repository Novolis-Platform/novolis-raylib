namespace Novolis.Raylib.Testing.Golden;

/// <summary>Outcome of <see cref="RaylibGoldenTest.Run"/>.</summary>
public sealed class GoldenTestResult
{
    public required bool Skipped { get; init; }

    public required bool Succeeded { get; init; }

    public bool AssertPassed { get; init; }

    public string? Message { get; init; }

    public string? ReviewReportPath { get; init; }

    public string? StoryDirectory { get; init; }

    public byte[]? ActualPng { get; init; }

    public GoldenPublishResult? MirrorPublish { get; init; }

    public static GoldenTestResult Skip(string reason, string? reviewReportPath = null) =>
        new()
        {
            Skipped = true,
            Succeeded = true,
            Message = reason,
            ReviewReportPath = reviewReportPath,
        };

    public static GoldenTestResult Fail(string message, string? reviewReportPath = null, string? storyDirectory = null) =>
        new()
        {
            Skipped = false,
            Succeeded = false,
            AssertPassed = false,
            Message = message,
            ReviewReportPath = reviewReportPath,
            StoryDirectory = storyDirectory,
        };

    public static GoldenTestResult Pass(
        string reviewReportPath,
        string storyDirectory,
        byte[] actualPng,
        bool assertPassed,
        GoldenPublishResult? mirrorPublish = null) =>
        new()
        {
            Skipped = false,
            Succeeded = true,
            AssertPassed = assertPassed,
            ReviewReportPath = reviewReportPath,
            StoryDirectory = storyDirectory,
            ActualPng = actualPng,
            MirrorPublish = mirrorPublish,
        };
}
