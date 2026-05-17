using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Frame renderer that can configure state before each golden frame capture.</summary>
public interface IGoldenStoryRenderer : IRaylibFrameRenderer
{
    /// <summary>Called before each frame harness sub-run (multi-frame stories).</summary>
    void BeginFrame(string frameId);
}
