using System.Text.RegularExpressions;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal static class RaylibManifestSuggester
{
    private static readonly Regex RlapiRegex = new(
        @"^\s*RLAPI\s+\w+\s+(\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static int Suggest(string repoRoot) =>
        Suggest(CodegenEnvironment.Physical(repoRoot), RaylibBindingManifestSource.Instance);

    public static int Suggest(CodegenEnvironment environment, IBindingManifestSource manifests)
    {
        var headerPath = PipelinePaths.RaylibHeaderPath(environment.RepoRoot);
        if (!environment.FileSystem.File.Exists(headerPath))
        {
            Console.Error.WriteLine($"Missing vendor header: {headerPath}");
            return 2;
        }

        var interop = manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "raylib6");
        var manifestNames = new HashSet<string>(
            interop.Imports.Select(i => i.Name),
            StringComparer.Ordinal);

        var header = environment.FileSystem.File.ReadAllText(headerPath);
        var missing = new List<string>();
        foreach (Match m in RlapiRegex.Matches(header))
        {
            var name = m.Groups[1].Value;
            if (!manifestNames.Contains(name))
                missing.Add(name);
        }

        missing.Sort(StringComparer.Ordinal);
        if (missing.Count == 0)
        {
            Console.WriteLine("suggest-raylib: manifest covers all RLAPI symbols found in raylib.h (by name).");
            return 0;
        }

        Console.WriteLine($"suggest-raylib: {missing.Count} RLAPI symbol(s) not in manifest (add template + row manually):");
        foreach (var name in missing)
            Console.WriteLine($"  {{ \"name\": \"{name}\", \"template\": \"<assign-template>\" }}");

        return 0;
    }
}
