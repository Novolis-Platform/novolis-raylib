using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>
/// Drains a main-thread work queue before/after scene setup and each frame
/// (typical for hosted games that post UI updates during startup).
/// </summary>
public sealed class QueuedGoldenStoryRenderer(
    IRaylibFrameRenderer inner,
    Action drainMainThreadQueue,
    IGoldenSceneScript sceneScript) : IGoldenStoryRenderer
{
    /// <inheritdoc />
    public void BeginFrame(string frameId)
    {
        drainMainThreadQueue();
        sceneScript.BeginFrame(frameId);
        drainMainThreadQueue();
    }

    /// <inheritdoc />
    public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight)
    {
        drainMainThreadQueue();
        inner.OnFrame(deltaSeconds, screenWidth, screenHeight);
    }
}
