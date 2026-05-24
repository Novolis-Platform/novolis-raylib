namespace Novolis.Raylib.Testing.Golden;

/// <summary>Output paths for one adhoc golden render run.</summary>
public sealed class GoldenRenderRunContext
{
    /// <summary>Top-level run folder shared by stories in one adhoc bucket.</summary>
    public required string RunFolder { get; init; }

    /// <summary>Directory for this story's QA bundle and PNGs.</summary>
    public required string StoryDirectory { get; init; }

    /// <summary>Sanitized test assembly name used in the path.</summary>
    public required string AssemblySegment { get; init; }

    /// <summary>Story identifier.</summary>
    public required string StoryId { get; init; }
}
