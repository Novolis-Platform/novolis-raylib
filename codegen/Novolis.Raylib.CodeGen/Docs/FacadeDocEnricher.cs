using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.CodeGen;

internal static class FacadeDocEnricher
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static int Enrich(string repoRoot, bool write)
    {
        var raylibComments = RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RaylibHeaderPath(repoRoot));
        var rayguiComments = RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RayguiHeaderPath(repoRoot));
        var pipelineDir = RepoPaths.PipelineDir(repoRoot);
        var manifestFiles = new[] { "facades.manifest.json", "hud.manifest.json", "gui.manifest.json" };
        var updated = 0;

        foreach (var file in manifestFiles)
        {
            var path = Path.Combine(pipelineDir, file);
            if (!File.Exists(path))
                continue;

            var json = File.ReadAllText(path);
            var doc = JsonSerializer.Deserialize<FacadesManifestDocument>(json, FacadeManifestModels.JsonReadOptions)
                      ?? throw new InvalidOperationException($"Failed to parse {path}");

            var changed = false;
            foreach (var type in doc.Types ?? [])
            {
                if (string.IsNullOrWhiteSpace(type.TypeSummary))
                {
                    var typeSummary = FacadeDocResolver.ResolveTypeSummary(type);
                    if (typeSummary is not null)
                    {
                        type.TypeSummary = typeSummary;
                        changed = true;
                    }
                }

                foreach (var method in type.Methods ?? [])
                {
                    if (!string.IsNullOrWhiteSpace(method.Summary))
                        continue;

                    method.Summary = FacadeDocResolver.ResolveMethodSummary(type, method, raylibComments, rayguiComments);
                    changed = true;
                }
            }

            if (changed)
            {
                updated++;
                if (write)
                {
                    var output = JsonSerializer.Serialize(doc, WriteOptions) + Environment.NewLine;
                    File.WriteAllText(path, output);
                    Console.WriteLine($"Updated {path}");
                }
                else
                {
                    Console.WriteLine($"Would update {path}");
                }
            }
        }

        if (updated == 0)
            Console.WriteLine("enrich-docs: all façade manifests already have summaries.");
        else if (!write)
            Console.WriteLine($"enrich-docs: {updated} manifest(s) need summaries. Re-run with --write to apply.");

        return 0;
    }
}

internal sealed class FacadesManifestDocument
{
    [JsonPropertyName("types")]
    public List<FacadeTypeDefinition>? Types { get; set; }
}
