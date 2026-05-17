using Novolis.Raylib.Diagnostics;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

var goldensRoot = Path.GetFullPath(Path.Combine(VisualCaptureArtifacts.FindRepoRoot(), "tests", "Novolis.Raylib.Golden", "Goldens"));
var assembly = typeof(Program).Assembly;

RaylibTestRuntime.EnableForAssembly();

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
            GoldensRoot = goldensRoot,
        });

    if (!result.Succeeded)
    {
        Console.Error.WriteLine($"Failed to seed {id}: {result.Message}");
        return 1;
    }

    Console.WriteLine($"Seeded {id}");
}

Console.WriteLine($"Committed baselines under {goldensRoot}");
return 0;
