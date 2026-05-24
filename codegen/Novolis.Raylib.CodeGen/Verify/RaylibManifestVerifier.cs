using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.CodeGen;

public static class RaylibManifestVerifier
{
    public static int Verify(string repoRoot)
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(repoRoot), "raylib-exports.manifest.json");
        var headerPath = PipelinePaths.RaylibHeaderPath(repoRoot);
        if (!File.Exists(manifestPath))
        {
            Console.Error.WriteLine($"Missing manifest: {manifestPath}");
            return 2;
        }

        if (!File.Exists(headerPath))
        {
            Console.WriteLine($"verify-raylib-manifest: skip (no vendor header at {headerPath}).");
            return 0;
        }

        var json = File.ReadAllText(manifestPath);
        var doc = JsonSerializer.Deserialize<ManifestRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (doc?.Imports is null || doc.Imports.Count == 0)
        {
            Console.Error.WriteLine("Manifest has no imports.");
            return 3;
        }

        var header = File.ReadAllText(headerPath);
        foreach (var imp in doc.Imports.OrderBy(i => i.Name, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(imp.Name))
                continue;

            if (!HeaderDeclaresSymbol(header, imp.Name))
            {
                Console.Error.WriteLine($"verify-raylib-manifest: '{imp.Name}' not found as RLAPI declaration in raylib.h.");
                return 4;
            }
        }

        Console.WriteLine($"verify-raylib-manifest: OK ({doc.Imports.Count} imports).");
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

internal sealed class ManifestRoot
{
    [JsonPropertyName("imports")]
    public List<ImportEntry>? Imports { get; set; }
}

internal sealed class ImportEntry
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
