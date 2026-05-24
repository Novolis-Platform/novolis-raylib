using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Entry point for golden output paths; delegates to <see cref="IGoldenRunBucketLayout"/>.</summary>
public static class GoldenRenderOutputLayout
{
    /// <summary>Default relative output root under the repo (<c>temp/test-renders</c>).</summary>
    public const string DefaultRelativeRoot = "temp/test-renders";

    /// <summary>Resolves run and story directories for a golden render.</summary>
    /// <param name="testAssembly">Test assembly for layout segmentation.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="outputRoot">Optional absolute output root.</param>
    /// <param name="runBucketLayout">Layout strategy (defaults to adhoc layout).</param>
    /// <returns>Resolved run context paths.</returns>
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

    /// <summary>Current shared adhoc run folder, if any.</summary>
    public static string? SharedRunFolder => GoldenAdhocRunBucketLayout.SharedRunFolder;

    /// <summary>Clears the shared adhoc run folder for a fresh bucket.</summary>
    public static void ResetSharedRun() => GoldenAdhocRunBucketLayout.ResetSharedRun();
}
