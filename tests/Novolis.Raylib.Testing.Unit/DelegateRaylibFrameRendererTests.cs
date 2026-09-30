using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class DelegateRaylibFrameRendererTests
{
    [Test]
    public async Task OnFrame_invokes_delegate()
    {
        float seenDt = 0;
        var seenW = 0;
        var seenH = 0;
        var renderer = new DelegateRaylibFrameRenderer((dt, w, h) =>
        {
            seenDt = dt;
            seenW = w;
            seenH = h;
        });
        renderer.OnFrame(0.016f, 800, 600);
        await Assert.That(seenDt).IsEqualTo(0.016f);
        await Assert.That(seenW).IsEqualTo(800);
        await Assert.That(seenH).IsEqualTo(600);
    }
}
