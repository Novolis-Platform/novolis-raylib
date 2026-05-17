namespace Novolis.Raylib.Testing.Golden;

/// <summary>Paths written by <see cref="GoldenArtifactPublisher.Publish"/>.</summary>
public sealed class GoldenPublishResult
{
    public required string DestinationDirectory { get; init; }

    public required string IndexHtmlPath { get; init; }

    public string IndexHtmlUri => new Uri(IndexHtmlPath).AbsoluteUri;

    public IReadOnlyList<string> CopiedPngPaths { get; init; } = [];
}
