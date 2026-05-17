using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Committed golden story metadata and QA expectations.</summary>
public sealed class GoldenStorySpec
{
    public const int CurrentSchemaVersion = 1;

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    [JsonPropertyName("storyId")]
    public required string StoryId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("width")]
    public int Width { get; init; } = 320;

    [JsonPropertyName("height")]
    public int Height { get; init; } = 240;

    [JsonPropertyName("maxFrames")]
    public int MaxFrames { get; init; } = 4;

    [JsonPropertyName("baselineSha256")]
    public string BaselineSha256 { get; set; } = "";

    [JsonPropertyName("expectations")]
    public IReadOnlyList<string> Expectations { get; init; } = [];

    [JsonIgnore]
    public string BaselinePngFileName => "baseline.png";

    public static GoldenStorySpec LoadFromFile(string specPath)
    {
        var json = File.ReadAllText(specPath);
        var spec = JsonSerializer.Deserialize<GoldenStorySpec>(json, JsonOptions)
                   ?? throw new InvalidOperationException($"Failed to parse {specPath}");
        if (string.IsNullOrWhiteSpace(spec.StoryId))
            throw new InvalidOperationException($"Missing storyId in {specPath}");
        return spec;
    }

    public void SaveToFile(string specPath)
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(specPath, json);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
