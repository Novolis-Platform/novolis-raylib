using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.CodeGen;

internal static class FacadeManifestModels
{
    internal static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    public static IReadOnlyList<FacadeTypeDefinition> LoadTypes(string manifestPath)
    {
        var json = File.ReadAllText(manifestPath);
        var doc = JsonSerializer.Deserialize<FacadesManifest>(json, JsonReadOptions)
                  ?? throw new InvalidOperationException($"Failed to parse {manifestPath}");
        if (doc.Types is null || doc.Types.Count == 0)
            throw new InvalidOperationException("facades.manifest.json has no types.");
        return doc.Types;
    }
}

internal sealed class FacadesManifest
{
    [JsonPropertyName("types")]
    public List<FacadeTypeDefinition>? Types { get; set; }
}

internal sealed class FacadeTypeDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = "";

    [JsonPropertyName("folder")]
    public string Folder { get; set; } = "";

    [JsonPropertyName("typeSummary")]
    public string? TypeSummary { get; set; }

    [JsonPropertyName("usings")]
    public List<string>? Usings { get; set; }

    [JsonPropertyName("methods")]
    public List<FacadeMethodDefinition>? Methods { get; set; }
}

internal sealed class FacadeMethodDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = "";

    [JsonPropertyName("body")]
    public string Body { get; set; } = "";

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }
}
