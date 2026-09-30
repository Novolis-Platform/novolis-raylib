using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Input;

namespace Novolis.Raylib.Testing.Unit;

public sealed class RaylibAbstractionsContractTests
{
    [Test]
    public async Task IRaylibFrameRenderer_OnFrame_ReceivesTimingAndSize()
    {
        float dt = 0;
        int w = 0;
        int h = 0;
        IRaylibFrameRenderer renderer = new StubFrameRenderer((delta, width, height) =>
        {
            dt = delta;
            w = width;
            h = height;
        });

        renderer.OnFrame(0.016f, 640, 480);

        await Assert.That(dt).IsEqualTo(0.016f);
        await Assert.That(w).IsEqualTo(640);
        await Assert.That(h).IsEqualTo(480);
    }

    private sealed class StubFrameRenderer(Action<float, int, int> onFrame) : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int width, int height) => onFrame(deltaSeconds, width, height);
    }
}
