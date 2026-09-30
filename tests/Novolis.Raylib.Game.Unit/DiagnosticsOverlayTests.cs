using System.Numerics;
using Novolis.Raylib.Game;

namespace Novolis.Raylib.Game.Unit;

public sealed class DiagnosticsOverlayTests
{
    [Test]
    public async Task Toggle_flips_visibility()
    {
        var overlay = new DiagnosticsOverlay();
        await Assert.That(overlay.Visible).IsTrue();
        overlay.Toggle();
        await Assert.That(overlay.Visible).IsFalse();
        overlay.Toggle();
        await Assert.That(overlay.Visible).IsTrue();
    }

    [Test]
    public async Task Draw_skips_rendering_when_hidden()
    {
        var overlay = new DiagnosticsOverlay();
        overlay.Toggle();
        var ctx = new RayGameContext(640, 480);
        ctx.SetScreen(800, 600, 1f / 60f);
        overlay.Draw(ctx);
        await Assert.That(overlay.Visible).IsFalse();
    }
}
