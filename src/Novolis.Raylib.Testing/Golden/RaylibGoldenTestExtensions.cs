using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Convenience wrappers around <see cref="RaylibGoldenTest"/>.</summary>
public static class RaylibGoldenTestExtensions
{
    public static (GoldenTestResult Result, GoldenPublishResult? Mirror) RunAndPublish(
        string storyId,
        IRaylibFrameRenderer renderer,
        GoldenRunOptions? options = null) =>
        RunAndPublish(storyId, new GoldenStoryRendererAdapter(renderer), options);

    public static (GoldenTestResult Result, GoldenPublishResult? Mirror) RunAndPublish(
        string storyId,
        IGoldenStoryRenderer renderer,
        GoldenRunOptions? options = null)
    {
        var result = RaylibGoldenTest.Run(storyId, renderer, options);
        return (result, result.MirrorPublish);
    }
}
