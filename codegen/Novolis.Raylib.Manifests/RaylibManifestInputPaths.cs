namespace Novolis.Raylib.Manifests;

public static class RaylibManifestInputPaths
{
    public static IReadOnlyList<string> AllManifestSourceFiles(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "codegen", "Novolis.Raylib.Manifests");
        return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();
    }
}
