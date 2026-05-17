namespace Novolis.Raylib.Testing.Golden;

/// <summary>Output paths for one adhoc golden render run.</summary>
public sealed class GoldenRenderRunContext
{
    public required string RunFolder { get; init; }

    public required string StoryDirectory { get; init; }

    public required string AssemblySegment { get; init; }

    public required string StoryId { get; init; }
}
