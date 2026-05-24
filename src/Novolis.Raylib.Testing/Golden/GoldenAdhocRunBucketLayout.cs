using System.Reflection;
using System.Text;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>
/// Default layout:
/// <c>{outputRoot}/adhoc-runs/{timestamp}_{pid}_{guid}/assemblies/{assembly}/renders/{storyId}/</c>
/// </summary>
public sealed class GoldenAdhocRunBucketLayout : IGoldenRunBucketLayout
{
    /// <summary>Shared singleton layout instance.</summary>
    public static GoldenAdhocRunBucketLayout Instance { get; } = new();

    private static readonly object RunLock = new();
    private static string? _sharedRunFolder;

    /// <inheritdoc />
    public GoldenRenderRunContext Resolve(Assembly testAssembly, string storyId, string outputRoot)
    {
        ArgumentNullException.ThrowIfNull(testAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(storyId);

        var root = string.IsNullOrWhiteSpace(outputRoot)
            ? Path.Combine(VisualCaptureArtifacts.FindRepoRoot(), GoldenRenderOutputLayout.DefaultRelativeRoot)
            : outputRoot;

        lock (RunLock)
        {
            _sharedRunFolder ??= CreateRunFolder(root);
            var assemblySegment = SanitizeAssemblyName(testAssembly.GetName().Name ?? "unknown");
            var storyDirectory = Path.Combine(
                _sharedRunFolder,
                "assemblies",
                assemblySegment,
                "renders",
                storyId);
            Directory.CreateDirectory(storyDirectory);

            return new GoldenRenderRunContext
            {
                RunFolder = _sharedRunFolder,
                StoryDirectory = storyDirectory,
                AssemblySegment = assemblySegment,
                StoryId = storyId,
            };
        }
    }

    /// <summary>Current shared adhoc run folder, or null before first resolve.</summary>
    public static string? SharedRunFolder
    {
        get
        {
            lock (RunLock)
                return _sharedRunFolder;
        }
    }

    /// <summary>Clears the shared run folder so the next resolve creates a new bucket.</summary>
    public static void ResetSharedRun()
    {
        lock (RunLock)
            _sharedRunFolder = null;
    }

    private static string CreateRunFolder(string outputRoot)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var pid = Environment.ProcessId;
        var guid = Guid.NewGuid().ToString("N");
        var runFolder = Path.Combine(outputRoot, "adhoc-runs", $"{stamp}_{pid:D5}_{guid}");
        Directory.CreateDirectory(runFolder);
        return runFolder;
    }

    private static string SanitizeAssemblyName(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                sb.Append(ch);
            else if (ch == '.' || ch == ' ')
                sb.Append('_');
        }

        return sb.Length > 0 ? sb.ToString() : "unknown";
    }
}
