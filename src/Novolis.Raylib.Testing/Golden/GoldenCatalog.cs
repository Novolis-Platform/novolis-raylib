using System.Reflection;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Resolves committed golden specs and baseline PNG paths.</summary>
public static class GoldenCatalog
{
    public const string GoldensFolderName = "Goldens";

    public static string GetStoryDirectory(Assembly testAssembly, string storyId, string? goldensRoot = null)
    {
        ArgumentNullException.ThrowIfNull(testAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(storyId);
        return Path.Combine(GetGoldensRoot(testAssembly, goldensRoot), storyId);
    }

    public static GoldenStorySpec LoadStory(Assembly testAssembly, string storyId, string? goldensRoot = null)
    {
        var dir = GetStoryDirectory(testAssembly, storyId, goldensRoot);
        var specPath = Path.Combine(dir, "spec.json");
        if (!File.Exists(specPath))
            throw new FileNotFoundException($"Golden spec not found: {specPath}", specPath);

        return GoldenStorySpec.LoadFromFile(specPath);
    }

    public static string GetBaselinePngPath(Assembly testAssembly, string storyId, string? goldensRoot = null) =>
        GetBaselinePngPath(testAssembly, storyId, frameId: null, goldensRoot);

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
