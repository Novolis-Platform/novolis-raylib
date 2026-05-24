using Novolis.Raylib.Abstractions;

namespace Novolis.Raylib.Testing;

/// <summary>Adapts a delegate to <see cref="IRaylibFrameRenderer"/> for tests and harness demos.</summary>
public sealed class DelegateRaylibFrameRenderer : IRaylibFrameRenderer
{
    private readonly Action<float, int, int> _onFrame;

    /// <summary>Creates a renderer that invokes <paramref name="onFrame"/> each frame.</summary>
    /// <param name="onFrame">Per-frame draw callback.</param>
    public DelegateRaylibFrameRenderer(Action<float, int, int> onFrame) =>
        _onFrame = onFrame ?? throw new ArgumentNullException(nameof(onFrame));

    /// <inheritdoc />
    public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) =>
        _onFrame(deltaSeconds, screenWidth, screenHeight);
}
