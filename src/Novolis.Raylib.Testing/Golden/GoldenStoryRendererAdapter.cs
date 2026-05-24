using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Adapts a plain <see cref="IRaylibFrameRenderer"/> for golden multi-frame runs.</summary>
public sealed class GoldenStoryRendererAdapter(IRaylibFrameRenderer inner) : IGoldenStoryRenderer
{
    /// <inheritdoc />
    public void BeginFrame(string frameId) { }

    /// <inheritdoc />
    public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) =>
        inner.OnFrame(deltaSeconds, screenWidth, screenHeight);
}
