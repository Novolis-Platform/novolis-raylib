using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

public static class RaylibManifestVerifier
{
    public static int Verify(string repoRoot) =>
        Verify(CodegenEnvironment.Physical(repoRoot), RaylibBindingManifestSource.Instance);

    public static int Verify(CodegenEnvironment environment, IBindingManifestSource manifests)
    {
        var interop = manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "raylib6");
        var headerPath = PipelinePaths.RaylibHeaderPath(environment.RepoRoot);
        if (!environment.FileSystem.File.Exists(headerPath))
        {
            Console.WriteLine($"verify-raylib-manifest: skip (no vendor header at {headerPath}).");
            return 0;
        }

        if (interop.Imports.Count == 0)
        {
            Console.Error.WriteLine("Manifest has no imports.");
            return 3;
        }

        var header = environment.FileSystem.File.ReadAllText(headerPath);
        foreach (var imp in interop.Imports.OrderBy(i => i.Name, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(imp.Name))
                continue;

            if (!HeaderDeclaresSymbol(header, imp.Name))
            {
                Console.Error.WriteLine($"verify-raylib-manifest: '{imp.Name}' not found as RLAPI declaration in raylib.h.");
                return 4;
            }
        }

        Console.WriteLine($"verify-raylib-manifest: OK ({interop.Imports.Count} imports).");
        return 0;
    }

    private static bool HeaderDeclaresSymbol(string header, string symbol)
    {
        var needle = symbol + "(";
        foreach (var raw in header.Split('\n'))
        {
            var line = raw.TrimStart();
            if (line.StartsWith("RLAPI", StringComparison.Ordinal) && line.Contains(needle, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
