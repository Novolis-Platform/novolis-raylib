using System.Text.Json.Serialization;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>One visual step in a multi-frame golden story.</summary>
public sealed class GoldenFrameSpec
{
    public const string DefaultFrameId = "default";

    [JsonPropertyName("frameId")]
    public required string FrameId { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("caption")]
    public string Caption { get; init; } = "";

    [JsonPropertyName("captureAtFrame")]
    public int CaptureAtFrame { get; init; }

    [JsonPropertyName("maxFrames")]
    public int MaxFrames { get; init; }

    [JsonPropertyName("baselineSha256")]
    public string BaselineSha256 { get; set; } = "";

    [JsonPropertyName("expectations")]
    public IReadOnlyList<string> Expectations { get; init; } = [];

    public int ResolveMaxFrames(int storyMaxFrames) =>
        MaxFrames > 0 ? MaxFrames : storyMaxFrames > 0 ? storyMaxFrames : 1;

    public int ResolveCaptureAtFrame(int resolvedMaxFrames) =>
        CaptureAtFrame > 0 ? CaptureAtFrame : resolvedMaxFrames;
}
