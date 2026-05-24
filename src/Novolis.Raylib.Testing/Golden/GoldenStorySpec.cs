using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Committed golden story metadata and QA expectations.</summary>
public sealed class GoldenStorySpec
{
    /// <summary>Current JSON schema version for <c>spec.json</c>.</summary>
    public const int CurrentSchemaVersion = 2;

    /// <summary>Schema version stored in committed specs.</summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>Unique story identifier and folder name.</summary>
    [JsonPropertyName("storyId")]
    public required string StoryId { get; init; }

    /// <summary>Display title in QA reports.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    /// <summary>Harness framebuffer width in pixels.</summary>
    [JsonPropertyName("width")]
    public int Width { get; init; } = 320;

    /// <summary>Harness framebuffer height in pixels.</summary>
    [JsonPropertyName("height")]
    public int Height { get; init; } = 240;

    /// <summary>Default max frames per harness sub-run.</summary>
    [JsonPropertyName("maxFrames")]
    public int MaxFrames { get; init; } = 4;

    /// <summary>Expected SHA-256 hex for single-frame baseline (optional).</summary>
    [JsonPropertyName("baselineSha256")]
    public string BaselineSha256 { get; set; } = "";

    /// <summary>QA checklist for single-frame stories.</summary>
    [JsonPropertyName("expectations")]
    public IReadOnlyList<string> Expectations { get; init; } = [];

    /// <summary>Per-frame specs for multi-frame stories.</summary>
    [JsonPropertyName("frames")]
    public IReadOnlyList<GoldenFrameSpec> Frames { get; init; } = [];

    /// <summary>True when <see cref="Frames"/> is non-empty.</summary>
    [JsonIgnore]
    public bool IsMultiFrame => Frames.Count > 0;

    /// <summary>Committed baseline file name for single-frame stories.</summary>
    [JsonIgnore]
    public string BaselinePngFileName => "baseline.png";

    /// <summary>Returns explicit frames or a synthetic single-frame spec.</summary>
    /// <returns>Frames to run and capture.</returns>
    public IReadOnlyList<GoldenFrameSpec> GetEffectiveFrames()
    {
        if (Frames.Count > 0)
            return Frames;

        return
        [
            new GoldenFrameSpec
            {
                FrameId = GoldenFrameSpec.DefaultFrameId,
                Title = string.IsNullOrWhiteSpace(Title) ? StoryId : Title,
                Caption = "Captured frame from this run.",
                MaxFrames = MaxFrames,
                CaptureAtFrame = 0,
                BaselineSha256 = BaselineSha256,
                Expectations = Expectations,
            },
        ];
    }

    /// <summary>Baseline PNG file name under committed <c>Goldens/</c>.</summary>
    /// <param name="spec">Story specification.</param>
    /// <param name="frame">Frame specification.</param>
    /// <returns>Committed baseline file name.</returns>
    public static string GetCommittedBaselineFileName(GoldenStorySpec spec, GoldenFrameSpec frame) =>
        spec.IsMultiFrame ? $"{frame.FrameId}.png" : spec.BaselinePngFileName;

    /// <summary>Actual PNG file name in adhoc QA output.</summary>
    /// <param name="spec">Story specification.</param>
    /// <param name="frame">Frame specification.</param>
    /// <returns>Adhoc actual PNG file name.</returns>
    public static string GetAdhocActualFileName(GoldenStorySpec spec, GoldenFrameSpec frame) =>
        spec.IsMultiFrame ? $"{frame.FrameId}.actual.png" : "actual.png";

    /// <summary>Baseline PNG file name copied into adhoc QA output.</summary>
    /// <param name="spec">Story specification.</param>
    /// <param name="frame">Frame specification.</param>
    /// <returns>Adhoc baseline PNG file name.</returns>
    public static string GetAdhocBaselineFileName(GoldenStorySpec spec, GoldenFrameSpec frame) =>
        spec.IsMultiFrame ? $"{frame.FrameId}.baseline.png" : "baseline.png";

    /// <summary>Loads a story spec from <c>spec.json</c>.</summary>
    /// <param name="specPath">Absolute path to the spec file.</param>
    /// <returns>Deserialized story specification.</returns>
    public static GoldenStorySpec LoadFromFile(string specPath)
    {
        var json = File.ReadAllText(specPath);
        var spec = JsonSerializer.Deserialize<GoldenStorySpec>(json, JsonOptions)
                   ?? throw new InvalidOperationException($"Failed to parse {specPath}");
        if (string.IsNullOrWhiteSpace(spec.StoryId))
            throw new InvalidOperationException($"Missing storyId in {specPath}");
        return spec;
    }

    /// <summary>Serializes this spec to <c>spec.json</c>.</summary>
    /// <param name="specPath">Absolute path to write.</param>
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
