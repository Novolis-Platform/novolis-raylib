using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing;

/// <summary>Adapts a delegate to <see cref="IRaylibFrameRenderer"/> for tests and harness demos.</summary>
public sealed class DelegateRaylibFrameRenderer : IRaylibFrameRenderer
{
    private readonly Action<float, int, int> _onFrame;

    public DelegateRaylibFrameRenderer(Action<float, int, int> onFrame) =>
        _onFrame = onFrame ?? throw new ArgumentNullException(nameof(onFrame));

    public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) =>
        _onFrame(deltaSeconds, screenWidth, screenHeight);
}
