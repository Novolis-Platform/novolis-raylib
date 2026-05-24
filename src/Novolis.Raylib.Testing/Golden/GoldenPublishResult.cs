namespace Novolis.Raylib.Testing.Golden;

/// <summary>Paths written by <see cref="GoldenArtifactPublisher.Publish"/>.</summary>
public sealed class GoldenPublishResult
{
    /// <summary>Absolute destination directory.</summary>
    public required string DestinationDirectory { get; init; }

    /// <summary>Absolute path to published <c>index.html</c>.</summary>
    public required string IndexHtmlPath { get; init; }

    /// <summary><c>file://</c> URI for <see cref="IndexHtmlPath"/>.</summary>
    public string IndexHtmlUri => new Uri(IndexHtmlPath).AbsoluteUri;

    /// <summary>Stable PNG paths copied during publish.</summary>
    public IReadOnlyList<string> CopiedPngPaths { get; init; } = [];
}
