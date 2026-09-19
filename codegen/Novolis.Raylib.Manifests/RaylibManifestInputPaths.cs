namespace Novolis.Raylib.Manifests;

public static class RaylibManifestInputPaths
{
    public static IReadOnlyList<string> AllManifestSourceFiles(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "codegen", "Novolis.Raylib.Manifests");
        return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
            {
                var rel = Path.GetRelativePath(dir, f);
                return !rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(segment => string.Equals(segment, "obj", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(segment, "bin", StringComparison.OrdinalIgnoreCase));
            })
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }
}
