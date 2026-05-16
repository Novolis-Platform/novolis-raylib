using System.Text.Json;
using System.Text.RegularExpressions;

namespace Novolis.Raylib.CodeGen;

internal static class RaylibManifestSuggester
{
    private static readonly Regex RlapiRegex = new(
        @"^\s*RLAPI\s+\w+\s+(\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public static int Suggest(string repoRoot)
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(repoRoot), "raylib-exports.manifest.json");
        var headerPath = Path.Combine(repoRoot, "tools", "vendor", "raylib-6", "include", "raylib.h");
        if (!File.Exists(headerPath))
        {
            Console.Error.WriteLine($"Missing vendor header: {headerPath}");
            return 2;
        }

        var manifestNames = new HashSet<string>(StringComparer.Ordinal);
        if (File.Exists(manifestPath))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(manifestPath));
            foreach (var el in doc.RootElement.GetProperty("imports").EnumerateArray())
                manifestNames.Add(el.GetProperty("name").GetString()!);
        }

        var header = File.ReadAllText(headerPath);
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
