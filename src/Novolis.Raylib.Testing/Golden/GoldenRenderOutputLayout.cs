using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Entry point for golden output paths; delegates to <see cref="IGoldenRunBucketLayout"/>.</summary>
public static class GoldenRenderOutputLayout
{
    public const string DefaultRelativeRoot = "temp/test-renders";

    public static GoldenRenderRunContext Resolve(
        Assembly testAssembly,
        string storyId,
        string? outputRoot = null,
        IGoldenRunBucketLayout? runBucketLayout = null) =>
        (runBucketLayout ?? GoldenAdhocRunBucketLayout.Instance).Resolve(
            testAssembly,
            storyId,
            string.IsNullOrWhiteSpace(outputRoot)
                ? Path.Combine(VisualCaptureArtifacts.FindRepoRoot(), DefaultRelativeRoot)
                : outputRoot);

    public static string? SharedRunFolder => GoldenAdhocRunBucketLayout.SharedRunFolder;

    public static void ResetSharedRun() => GoldenAdhocRunBucketLayout.ResetSharedRun();
}
