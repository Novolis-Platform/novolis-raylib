using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Integration;

/// <summary>Thin integration wrapper over <see cref="RaylibGoldenTest"/> (smoke scene).</summary>
public sealed class VisualSmokeCaptureTests
{
    [Test]
    public async Task Capture_smoke_scene_writes_golden_review_bundle()
    {
        RaylibTestRuntime.EnableForAssembly();

        var result = RaylibGoldenTest.Run(
            "raylib-golden-smoke-scene",
            new DelegateRaylibFrameRenderer(GoldenScenes.DrawSmokeScene),
            new GoldenRunOptions
            {
                Mode = GoldenRunMode.ReportOnly,
                TestAssembly = typeof(VisualSmokeCaptureTests).Assembly,
            });

        if (result.Skipped)
        {
            Console.WriteLine($"Skipped: {result.Message}");
            await Assert.That(result.ReviewReportPath).IsNotNull();
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.ReviewReportPath).IsNotNull();
        Console.WriteLine($"Golden render report: {result.ReviewReportPath}");
    }
}
