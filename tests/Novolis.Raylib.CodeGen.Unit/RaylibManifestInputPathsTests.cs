using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibManifestInputPathsTests
{
    [Test]
    public async Task AllManifestSourceFiles_IncludesKnownManifestTypes()
    {
        var repoRoot = FindRepoRoot();
        var files = RaylibManifestInputPaths.AllManifestSourceFiles(repoRoot);

        await Assert.That(files.Count).IsGreaterThan(0);
        await Assert.That(files.Any(f => f.EndsWith("Raylib6InteropManifest.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(files.Any(f => f.EndsWith("FacadesManifest.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    [Test]
    public async Task AllManifestSourceFiles_IsDeterministic()
    {
        var repoRoot = FindRepoRoot();
        var first = RaylibManifestInputPaths.AllManifestSourceFiles(repoRoot);
        var second = RaylibManifestInputPaths.AllManifestSourceFiles(repoRoot);
        await Assert.That(first).IsEquivalentTo(second);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "codegen", "Novolis.Raylib.Manifests")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate novolis-raylib repo root.");
    }
}
