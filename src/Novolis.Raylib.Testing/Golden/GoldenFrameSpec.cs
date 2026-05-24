using System.Text.Json.Serialization;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>One visual step in a multi-frame golden story.</summary>
public sealed class GoldenFrameSpec
{
    /// <summary>Default frame id for single-frame stories.</summary>
    public const string DefaultFrameId = "default";

    /// <summary>Unique frame identifier within the story.</summary>
    [JsonPropertyName("frameId")]
    public required string FrameId { get; init; }

    /// <summary>Display title in QA reports.</summary>
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    /// <summary>Short description shown under the title in reports.</summary>
    [JsonPropertyName("caption")]
    public string Caption { get; init; } = "";

    /// <summary>1-based frame index to capture (0 = last frame).</summary>
    [JsonPropertyName("captureAtFrame")]
    public int CaptureAtFrame { get; init; }

    /// <summary>Max frames for this step (0 = inherit from story).</summary>
    [JsonPropertyName("maxFrames")]
    public int MaxFrames { get; init; }

    /// <summary>Expected SHA-256 hex of baseline PNG (optional).</summary>
    [JsonPropertyName("baselineSha256")]
    public string BaselineSha256 { get; set; } = "";

    /// <summary>Human-readable QA checklist items for this frame.</summary>
    [JsonPropertyName("expectations")]
    public IReadOnlyList<string> Expectations { get; init; } = [];

    /// <summary>Resolves max frames using frame override or story default.</summary>
    /// <param name="storyMaxFrames">Story-level max frames.</param>
    /// <returns>Effective max frames (at least 1).</returns>
    public int ResolveMaxFrames(int storyMaxFrames) =>
        MaxFrames > 0 ? MaxFrames : storyMaxFrames > 0 ? storyMaxFrames : 1;

    /// <summary>Resolves capture frame index using frame override or last frame.</summary>
    /// <param name="resolvedMaxFrames">Effective max frames for this step.</param>
    /// <returns>1-based frame index to capture.</returns>
    public int ResolveCaptureAtFrame(int resolvedMaxFrames) =>
        CaptureAtFrame > 0 ? CaptureAtFrame : resolvedMaxFrames;
}
