using System.Reflection;
using System.Text;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>
/// Star Conflicts Revolt-style layout:
/// temp/test-renders/adhoc-runs/{timestamp}_{pid}_{guid}/assemblies/{assembly}/renders/{storyId}/
/// </summary>
public static class GoldenRenderOutputLayout
{
    public const string DefaultRelativeRoot = "temp/test-renders";

    private static readonly object RunLock = new();
    private static string? _sharedRunFolder;

    public static GoldenRenderRunContext Resolve(Assembly testAssembly, string storyId, string? outputRoot = null)
    {
        ArgumentNullException.ThrowIfNull(testAssembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(storyId);

        var root = string.IsNullOrWhiteSpace(outputRoot)
            ? Path.Combine(VisualCaptureArtifacts.FindRepoRoot(), DefaultRelativeRoot)
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

    public static string? SharedRunFolder
    {
        get
        {
            lock (RunLock)
                return _sharedRunFolder;
        }
    }

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
