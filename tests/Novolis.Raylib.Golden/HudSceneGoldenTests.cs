using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class HudSceneGoldenTests
{
    [Test]
    [Category("Golden")]
    [Category("Native")]
    [RunOnlyIfNativeRaylib]
    public async Task Hud_text_scene_matches_golden_baseline()
    {
        NativeRaylibTestGate.EnsureAvailable();

        var result = RaylibGoldenTest.Run(
            "raylib-golden-hud-scene",
            new DelegateRaylibFrameRenderer(GoldenScenes.DrawHudScene),
            new GoldenRunOptions { TestAssembly = typeof(HudSceneGoldenTests).Assembly });

        if (result.Skipped)
        {
            await Assert.That(result.ReviewReportPath).IsNotNull();
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.AssertPassed).IsTrue();
    }
}
