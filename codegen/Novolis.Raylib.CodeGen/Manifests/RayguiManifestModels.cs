using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.CodeGen;

internal static class RayguiManifestModels
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<RayguiFunction> LoadFunctions(string manifestPath)
    {
        var json = File.ReadAllText(manifestPath);
        var doc = JsonSerializer.Deserialize<RayguiManifest>(json, JsonOptions)
                  ?? throw new InvalidOperationException($"Failed to parse {manifestPath}");
        if (doc.Functions is null || doc.Functions.Count == 0)
            throw new InvalidOperationException("Manifest has no functions.");
        return doc.Functions;
    }
}

internal sealed class RayguiManifest
{
    [JsonPropertyName("functions")]
    public List<RayguiFunction>? Functions { get; set; }
}

internal sealed class RayguiFunction
{
    [JsonPropertyName("export")]
    public string? Export { get; set; }

    [JsonPropertyName("template")]
    public string? Template { get; set; }
}
