namespace Novolis.Raylib.Testing;

/// <summary>Writes offscreen PNG captures under <c>artifacts/visual-captures/</c> for human or agent review.</summary>
public static class VisualCaptureArtifacts
{
    /// <summary>Relative path from repo root to legacy visual capture folder.</summary>
    public const string RelativeCapturesDir = "artifacts/visual-captures";

    /// <summary>Absolute path to the legacy captures directory under the repo root.</summary>
    [Obsolete("Prefer RaylibGoldenTest with GoldenRunMode.ReportOnly; writes QA bundles under temp/test-renders/.")]
    public static string CapturesDirectory => Path.Combine(FindRepoRoot(), RelativeCapturesDir);

    /// <summary>Writes PNG bytes to the legacy captures directory.</summary>
    /// <param name="png">PNG file contents.</param>
    /// <param name="fileName">File name (`.png` appended when missing).</param>
    /// <returns>Absolute path to the written file.</returns>
    [Obsolete("Prefer RaylibGoldenTest with GoldenRunMode.ReportOnly; writes QA bundles under temp/test-renders/.")]
    public static string WritePng(ReadOnlySpan<byte> png, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            fileName += ".png";

        var dir = CapturesDirectory;
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        File.WriteAllBytes(path, png);
        return Path.GetFullPath(path);
    }

    /// <summary>Walks upward from the app base directory to locate the Novolis.Raylib repo root.</summary>
    /// <returns>Directory containing <c>Novolis.Raylib.slnx</c>, or current working directory.</returns>
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Novolis.Raylib.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (cwd is not null)
        {
            if (File.Exists(Path.Combine(cwd.FullName, "Novolis.Raylib.slnx")))
                return cwd.FullName;
            cwd = cwd.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
