namespace Novolis.Raylib.Testing;

/// <summary>Writes offscreen PNG captures under <c>artifacts/visual-captures/</c> for human or agent review.</summary>
public static class VisualCaptureArtifacts
{
    public const string RelativeCapturesDir = "artifacts/visual-captures";

    public static string CapturesDirectory => Path.Combine(FindRepoRoot(), RelativeCapturesDir);

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
