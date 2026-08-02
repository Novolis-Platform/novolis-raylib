using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Input;

namespace Novolis.Raylib.Testing.Unit;

public sealed class NullInputSourceTests
{
    [Test]
    public void StartStop_AreNoOps()
    {
        var input = new NullInputSource();
        input.Start();
        input.Stop();
    }

    [Test]
    public void EventSubscriptions_AcceptCallbacksWithoutInvoking()
    {
        var input = new NullInputSource();
        input.OnMouseMove(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnMouseClick(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnKeyPress(_ => throw new InvalidOperationException("Should not invoke"));
        input.OnKeyRelease(_ => throw new InvalidOperationException("Should not invoke"));
    }
}

public sealed class InputEventArgsTests
{
    [Test]
    public async Task MouseEventArgs_StoresPayload()
    {
        var args = new MouseEventArgs(10, 20, 1);
        await Assert.That(args.X).IsEqualTo(10);
        await Assert.That(args.Y).IsEqualTo(20);
        await Assert.That(args.Button).IsEqualTo(1);
    }

    [Test]
    public async Task KeyboardEventArgs_StoresKeyCode()
    {
        var args = new KeyboardEventArgs(32);
        await Assert.That(args.KeyCode).IsEqualTo(32);
    }
}

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
