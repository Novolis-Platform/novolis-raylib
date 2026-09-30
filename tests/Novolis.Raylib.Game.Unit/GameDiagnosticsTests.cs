using System.Numerics;
using Novolis.Raylib.Game;

namespace Novolis.Raylib.Game.Unit;

public sealed class FrameDiagnosticsTests
{
    [Test]
    public async Task Capture_populates_fps_and_timing_from_inputs()
    {
        var diag = FrameDiagnostics.Capture(smoothedFps: 60f, deltaSeconds: 1f / 60f);
        await Assert.That(diag.SmoothedFps).IsEqualTo(60f);
        await Assert.That(diag.FrameMilliseconds).IsGreaterThan(16f);
        await Assert.That(diag.FrameMilliseconds).IsLessThan(17f);
        await Assert.That(diag.WorkingSetMegabytes).IsGreaterThan(0);
        await Assert.That(diag.GcHeapMegabytes).IsGreaterThanOrEqualTo(0);
    }
}
