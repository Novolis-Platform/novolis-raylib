namespace Novolis.Raylib.Testing.Golden;

/// <summary>Options for <see cref="GoldenArtifactPublisher.Publish"/>.</summary>
public sealed class GoldenPublishOptions
{
    /// <summary>When true, copies <c>{frameId}.actual.png</c> to <c>{frameId}.png</c>.</summary>
    public bool StablePngNames { get; init; } = true;

    /// <summary>When true, copies <c>manifest.json</c>, <c>expectations.md</c>, and <c>agent-brief.json</c>.</summary>
    public bool CopySidecarFiles { get; init; } = true;

    /// <summary>When true, writes <c>README.txt</c> with index path and file URI.</summary>
    public bool WriteReadme { get; init; } = true;

    /// <summary>Optional step summary for README (e.g. <c>01-galaxy → 02-production</c>).</summary>
    public string? ReadmeStepSummary { get; init; }

    /// <summary>Title for fallback HTML when source has no <c>index.html</c>.</summary>
    public string FallbackTitle { get; init; } = "Raylib golden story";

    /// <summary>Status line when fallback HTML is written for a skipped run.</summary>
    public string? SkippedMessage { get; set; }

    /// <summary>Status line when fallback HTML is written for a failed run.</summary>
    public string? FailedMessage { get; set; }

    /// <summary>When true, fallback HTML notes report-only mode.</summary>
    public bool ReportOnly { get; set; }
}
