using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class SmokeSceneGoldenTests
{
    [Test]
    [Category("Golden")]
    [Category("Native")]
    [RunOnlyIfNativeRaylib]
    public async Task Smoke_scene_matches_golden_baseline()
    {
        NativeRaylibTestGate.EnsureAvailable();

        var result = RaylibGoldenTest.Run(
            "raylib-golden-smoke-scene",
            new DelegateRaylibFrameRenderer(GoldenScenes.DrawSmokeScene),
            new GoldenRunOptions { TestAssembly = typeof(SmokeSceneGoldenTests).Assembly });

        if (result.Skipped)
        {
            await Assert.That(result.ReviewReportPath).IsNotNull();
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.AssertPassed).IsTrue();
        await Assert.That(result.ReviewReportPath).IsNotNull();
        await Assert.That(File.Exists(result.ReviewReportPath!)).IsTrue();
    }
}
