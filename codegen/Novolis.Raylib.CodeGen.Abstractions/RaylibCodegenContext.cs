using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen;

public sealed class RaylibCodegenContext
{
    public required string RepoRoot { get; init; }

    public required RaylibCodegenPhase Phase { get; init; }

    public required string OutputPath { get; init; }

    public required string ManifestPath { get; init; }

    public required string ManifestSha256 { get; init; }

    public required string RegenerateHint { get; init; }

    public IReadOnlyDictionary<string, string> ImportDescriptions { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public string? FacadeTypeName { get; init; }

    /// <summary>From <c>interopPolicy.facadeMethodImpl</c> (e.g. <c>AggressiveInlining</c>).</summary>
    public string? FacadeMethodImpl { get; init; }
}
