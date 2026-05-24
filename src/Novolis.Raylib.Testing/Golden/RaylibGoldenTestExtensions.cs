using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Convenience wrappers around <see cref="RaylibGoldenTest"/>.</summary>
public static class RaylibGoldenTestExtensions
{
    /// <summary>Runs a golden story and returns the result with mirror publish info.</summary>
    /// <param name="storyId">Committed story identifier.</param>
    /// <param name="renderer">Per-frame draw callback.</param>
    /// <param name="options">Run options (defaults when null).</param>
    /// <returns>Test result and optional mirror publish outcome.</returns>
    public static (GoldenTestResult Result, GoldenPublishResult? Mirror) RunAndPublish(
        string storyId,
        IRaylibFrameRenderer renderer,
        GoldenRunOptions? options = null) =>
        RunAndPublish(storyId, new GoldenStoryRendererAdapter(renderer), options);

    /// <summary>Runs a golden story and returns the result with mirror publish info.</summary>
    /// <param name="storyId">Committed story identifier.</param>
    /// <param name="renderer">Story renderer with per-frame setup.</param>
    /// <param name="options">Run options (defaults when null).</param>
    /// <returns>Test result and optional mirror publish outcome.</returns>
    public static (GoldenTestResult Result, GoldenPublishResult? Mirror) RunAndPublish(
        string storyId,
        IGoldenStoryRenderer renderer,
        GoldenRunOptions? options = null)
    {
        var result = RaylibGoldenTest.Run(storyId, renderer, options);
        return (result, result.MirrorPublish);
    }
}
