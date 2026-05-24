using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Resolves committed golden specs and baseline PNG paths.</summary>
public static class GoldenCatalog
{
    /// <summary>Default folder name for committed golden assets.</summary>
    public const string GoldensFolderName = "Goldens";

    /// <summary>Directory containing one story's spec and baseline PNGs.</summary>
    /// <param name="testAssembly">Test assembly for root discovery.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="goldensRoot">Optional override for committed goldens root.</param>
    /// <returns>Absolute story directory path.</returns>
    public static string GetStoryDirectory(Assembly testAssembly, string storyId, string? goldensRoot = null)
    {
        ArgumentNullException.ThrowIfNull(testAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(storyId);
        return Path.Combine(GetGoldensRoot(testAssembly, goldensRoot), storyId);
    }

    /// <summary>Loads <c>spec.json</c> for a committed golden story.</summary>
    /// <param name="testAssembly">Test assembly for root discovery.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="goldensRoot">Optional override for committed goldens root.</param>
    /// <returns>Deserialized story specification.</returns>
    public static GoldenStorySpec LoadStory(Assembly testAssembly, string storyId, string? goldensRoot = null)
    {
        var dir = GetStoryDirectory(testAssembly, storyId, goldensRoot);
        var specPath = Path.Combine(dir, "spec.json");
        if (!File.Exists(specPath))
            throw new FileNotFoundException($"Golden spec not found: {specPath}", specPath);

        return GoldenStorySpec.LoadFromFile(specPath);
    }

    /// <summary>Path to the single-frame baseline PNG.</summary>
    /// <param name="testAssembly">Test assembly for root discovery.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="goldensRoot">Optional override for committed goldens root.</param>
    /// <returns>Absolute baseline PNG path.</returns>
    public static string GetBaselinePngPath(Assembly testAssembly, string storyId, string? goldensRoot = null) =>
        GetBaselinePngPath(testAssembly, storyId, frameId: null, goldensRoot);

    /// <summary>Path to a committed baseline PNG for one frame.</summary>
    /// <param name="testAssembly">Test assembly for root discovery.</param>
    /// <param name="storyId">Story identifier.</param>
    /// <param name="frameId">Frame id for multi-frame stories (null for single-frame).</param>
    /// <param name="goldensRoot">Optional override for committed goldens root.</param>
    /// <returns>Absolute baseline PNG path.</returns>
    public static string GetBaselinePngPath(
        Assembly testAssembly,
        string storyId,
        string? frameId,
        string? goldensRoot = null)
    {
        var storyDir = GetStoryDirectory(testAssembly, storyId, goldensRoot);
        if (string.IsNullOrWhiteSpace(frameId) || frameId == GoldenFrameSpec.DefaultFrameId)
            return Path.Combine(storyDir, "baseline.png");

        return Path.Combine(storyDir, $"{frameId}.png");
    }

    /// <summary>Locates the committed <c>Goldens/</c> root for a test assembly.</summary>
    /// <param name="testAssembly">Test assembly for probing.</param>
    /// <param name="goldensRootOverride">Explicit root when set.</param>
    /// <returns>Absolute goldens root directory.</returns>
    public static string GetGoldensRoot(Assembly testAssembly, string? goldensRootOverride = null)
    {
        if (!string.IsNullOrWhiteSpace(goldensRootOverride))
            return Path.GetFullPath(goldensRootOverride);

        var dir = Path.GetDirectoryName(testAssembly.Location);
        if (!string.IsNullOrEmpty(dir))
        {
            var fromOutput = Path.Combine(dir, GoldensFolderName);
            if (Directory.Exists(fromOutput))
                return fromOutput;

            var probe = new DirectoryInfo(dir);
            for (var i = 0; i < 8 && probe is not null; i++, probe = probe.Parent)
            {
                var candidate = Path.Combine(probe.FullName, GoldensFolderName);
                if (Directory.Exists(candidate))
                    return candidate;
            }
        }

        var repoGoldens = Path.Combine(VisualCaptureArtifacts.FindRepoRoot(), "tests", "Novolis.Raylib.Golden", GoldensFolderName);
        if (Directory.Exists(repoGoldens))
            return repoGoldens;

        throw new DirectoryNotFoundException(
            $"Could not locate {GoldensFolderName} for assembly {testAssembly.GetName().Name}.");
    }
}
