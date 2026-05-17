using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

/// <summary>Run explicitly to refresh committed baseline PNGs and spec hashes.</summary>
public sealed class UpdateBaselinesTests
{
    [Test]
    [Explicit]
    public async Task Update_all_golden_baselines()
    {
        RaylibTestRuntime.EnableForAssembly();
        var assembly = typeof(UpdateBaselinesTests).Assembly;
        var stories = new (string Id, DelegateRaylibFrameRenderer Renderer)[]
        {
            ("raylib-golden-smoke-scene", new DelegateRaylibFrameRenderer(GoldenScenes.DrawSmokeScene)),
            ("raylib-golden-hud-scene", new DelegateRaylibFrameRenderer(GoldenScenes.DrawHudScene)),
            ("raylib-golden-world-cube", new DelegateRaylibFrameRenderer(GoldenScenes.DrawWorldCube)),
        };

        foreach (var (id, renderer) in stories)
        {
            var result = RaylibGoldenTest.Run(
                id,
                renderer,
                new GoldenRunOptions
                {
                    Mode = GoldenRunMode.UpdateBaselines,
                    TestAssembly = assembly,
                });
            await Assert.That(result.Succeeded).IsTrue();
        }
    }
}
