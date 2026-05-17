using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class WorldCubeGoldenTests
{
    [Test]
    [Category("Golden")]
    [Category("Native")]
    [RunOnlyIfNativeRaylib]
    public async Task World_cube_scene_matches_golden_baseline()
    {
        var result = RaylibGoldenTest.Run(
            "raylib-golden-world-cube",
            new DelegateRaylibFrameRenderer(GoldenScenes.DrawWorldCube),
            new GoldenRunOptions { TestAssembly = typeof(WorldCubeGoldenTests).Assembly });

        if (result.Skipped)
        {
            await Assert.That(result.ReviewReportPath).IsNotNull();
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.AssertPassed).IsTrue();
    }
}
