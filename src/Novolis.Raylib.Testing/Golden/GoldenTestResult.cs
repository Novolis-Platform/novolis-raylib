namespace Novolis.Raylib.Testing.Golden;

/// <summary>Outcome of <see cref="RaylibGoldenTest.Run(string, Novolis.Raylib.Abstractions.IRaylibFrameRenderer, GoldenRunOptions?)"/>.</summary>
public sealed class GoldenTestResult
{
    /// <summary>True when the run was skipped (not a test failure).</summary>
    public required bool Skipped { get; init; }

    /// <summary>True when the run completed without unhandled failure.</summary>
    public required bool Succeeded { get; init; }

    /// <summary>True when baseline comparison passed (when asserting).</summary>
    public bool AssertPassed { get; init; }

    /// <summary>Skip, failure, or status message.</summary>
    public string? Message { get; init; }

    /// <summary>Absolute path to generated <c>index.html</c> QA report.</summary>
    public string? ReviewReportPath { get; init; }

    /// <summary>Absolute path to the story output directory.</summary>
    public string? StoryDirectory { get; init; }

    /// <summary>PNG bytes from the last captured frame.</summary>
    public byte[]? ActualPng { get; init; }

    /// <summary>Mirror publish result when configured.</summary>
    public GoldenPublishResult? MirrorPublish { get; init; }

    /// <summary>Creates a skipped golden test result.</summary>
    /// <param name="reason">Skip reason.</param>
    /// <param name="reviewReportPath">Optional QA report path.</param>
    /// <returns>Skipped result marked as succeeded.</returns>
    public static GoldenTestResult Skip(string reason, string? reviewReportPath = null) =>
        new()
        {
            Skipped = true,
            Succeeded = true,
            Message = reason,
            ReviewReportPath = reviewReportPath,
        };

    /// <summary>Creates a failed golden test result.</summary>
    /// <param name="message">Failure message.</param>
    /// <param name="reviewReportPath">Optional QA report path.</param>
    /// <param name="storyDirectory">Optional story output directory.</param>
    /// <returns>Failed result.</returns>
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

    /// <summary>Creates a successful golden test result.</summary>
    /// <param name="reviewReportPath">QA report path.</param>
    /// <param name="storyDirectory">Story output directory.</param>
    /// <param name="actualPng">Last captured PNG bytes.</param>
    /// <param name="assertPassed">Whether baseline comparison passed.</param>
    /// <param name="mirrorPublish">Optional mirror publish result.</param>
    /// <returns>Successful result.</returns>
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
