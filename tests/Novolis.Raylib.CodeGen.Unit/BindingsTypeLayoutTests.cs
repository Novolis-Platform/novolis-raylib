using System.Numerics;
using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class BindingsTypeLayoutTests
{
    [Test]
    public async Task Camera_perspective_factory_sets_projection()
    {
        var cam = Camera.Perspective(new(0, 2, 0), Vector3.Zero, Vector3.UnitY, 45f);
        await Assert.That(cam.Projection).IsEqualTo(CameraProjection.Perspective);
        await Assert.That(cam.Fovy).IsEqualTo(45f);
    }

    [Test]
    public async Task RaylibColor_struct_size_is_four_bytes()
    {
        await Assert.That(Marshal.SizeOf<RaylibColor>()).IsEqualTo(4);
    }
}

public sealed class RaylibManifestSuggesterUnitTests
{
    [Test]
    public async Task Suggest_returns_2_when_header_missing()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-codegen-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tempRoot, "pipeline", "raylib6"));
        var code = RaylibManifestSuggester.Suggest(tempRoot);
        await Assert.That(code).IsEqualTo(2);
        Directory.Delete(tempRoot, recursive: true);
    }

    [Test]
    public async Task Suggest_reports_missing_symbols()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-codegen-unit", Guid.NewGuid().ToString("N"));
        var pipelineDir = Path.Combine(tempRoot, "pipeline", "raylib6");
        var artifacts = Path.Combine(pipelineDir, "steps", "step_01_source", "artifacts", "raylib-6", "include");
        Directory.CreateDirectory(artifacts);
        File.WriteAllText(Path.Combine(pipelineDir, "raylib-exports.manifest.json"), """{"imports":[{"name":"InitWindow"}]}""");
        File.WriteAllText(
            Path.Combine(artifacts, "raylib.h"),
            "RLAPI void InitWindow(int w, int h, const char* t);\nRLAPI void CloseWindow(void);\n");
        var code = RaylibManifestSuggester.Suggest(tempRoot);
        await Assert.That(code).IsEqualTo(0);
        Directory.Delete(tempRoot, recursive: true);
    }
}
