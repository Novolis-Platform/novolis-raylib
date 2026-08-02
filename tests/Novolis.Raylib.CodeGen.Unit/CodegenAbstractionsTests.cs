using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibPipelineLayoutTests
{
    [Test]
    public async Task Find_resolves_repo_with_packages_props()
    {
        var layout = RaylibPipelineLayout.Find();
        await Assert.That(File.Exists(Path.Combine(layout.RepoRoot, "Directory.Packages.props"))).IsTrue();
    }

    [Test]
    public async Task StepDir_and_artifacts_follow_pipeline_convention()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var layout = new RaylibPipelineLayout(repoRoot);
        await Assert.That(layout.StepsRoot).Contains("pipeline");
        await Assert.That(layout.StepDir("step_03_verify_manifest"))
            .IsEqualTo(PipelinePaths.StepDir(repoRoot, "step_03_verify_manifest"));
        await Assert.That(layout.StepArtifactsDir("step_01_source"))
            .IsEqualTo(PipelinePaths.StepArtifactsDir(repoRoot, "step_01_source"));
    }
}

public sealed class PipelinePathsExtendedTests
{
    [Test]
    public async Task Path_helpers_resolve_under_temp_repo_root()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "novolis-codegen-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repoRoot);
        File.WriteAllText(Path.Combine(repoRoot, "Directory.Packages.props"), "<Project/>");

        try
        {
            await Assert.That(PipelinePaths.CodegenRoot(repoRoot)).EndsWith("codegen");
            await Assert.That(PipelinePaths.PipelineRaylibDir(repoRoot)).Contains("raylib6");
            await Assert.That(PipelinePaths.VersionsJson(repoRoot)).EndsWith("versions.json");
            await Assert.That(PipelinePaths.RaylibHeaderPath(repoRoot)).EndsWith("raylib.h");
            await Assert.That(PipelinePaths.RayguiHeaderPath(repoRoot)).EndsWith("raygui.h");
            await Assert.That(PipelinePaths.NativeRoot(repoRoot)).EndsWith("native");
            await Assert.That(PipelinePaths.VendorRoot(repoRoot)).EndsWith("vendor");
            await Assert.That(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"))
                .Contains("raylib6-platform");
            await Assert.That(PipelinePaths.RaylibNativeAndroidArm64Dir(repoRoot))
                .Contains("android-arm64");
        }
        finally
        {
            Directory.Delete(repoRoot, recursive: true);
        }
    }

    [Test]
    public async Task FindRepoRoot_prefers_packages_props_over_cwd()
    {
        var root = PipelinePaths.FindRepoRoot();
        await Assert.That(Directory.Exists(root)).IsTrue();
    }
}

public sealed class RaylibColorsTests
{
    [Test]
    public async Task Presets_match_raylib_palette_values()
    {
        var rayWhite = RaylibColors.RayWhite;
        var darkGray = RaylibColors.DarkGray;
        await Assert.That((int)rayWhite.A).IsEqualTo(255);
        await Assert.That((int)rayWhite.R).IsEqualTo(245);
        await Assert.That((int)darkGray.G).IsEqualTo(80);
        await Assert.That(RaylibColors.White).IsEqualTo(System.Drawing.Color.White);
        await Assert.That(RaylibColors.Black).IsEqualTo(System.Drawing.Color.Black);
    }
}

public sealed class RaylibVector3Tests
{
    [Test]
    public async Task ForwardFromYawPitch_at_zero_points_down_negative_z()
    {
        var forward = RaylibVector3.ForwardFromYawPitch(0f, 0f);
        await Assert.That(forward.X).IsEqualTo(0f).Within(0.001f);
        await Assert.That(forward.Y).IsEqualTo(0f).Within(0.001f);
        await Assert.That(forward.Z).IsEqualTo(-1f).Within(0.001f);
    }

    [Test]
    public async Task ForwardFromYawPitch_returns_unit_length()
    {
        var forward = RaylibVector3.ForwardFromYawPitch(0.7f, -0.3f);
        var length = forward.Length();
        await Assert.That(length).IsEqualTo(1f).Within(0.001f);
    }
}

public sealed class RaylibDebugCaptureGateTests
{
    [Test]
    public async Task IsRequested_true_when_programmatic_enabled()
    {
        RaylibDebugCaptureGate.ProgrammaticEnabled = true;
        try
        {
            await Assert.That(RaylibDebugCaptureGate.IsRequested("NOVOLIS_UNUSED")).IsTrue();
        }
        finally
        {
            RaylibDebugCaptureGate.ProgrammaticEnabled = false;
        }
    }

    [Test]
    public async Task IsRequested_honors_environment_variable()
    {
        RaylibDebugCaptureGate.ProgrammaticEnabled = false;
        Environment.SetEnvironmentVariable("NOVOLIS_CAPTURE_GATE_TEST", "yes");
        try
        {
            await Assert.That(RaylibDebugCaptureGate.IsRequested("NOVOLIS_CAPTURE_GATE_TEST")).IsTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_CAPTURE_GATE_TEST", null);
        }
    }
}

public sealed class TextureWrapperTests
{
    [Test]
    public async Task FromNative_maps_handle_fields()
    {
        var native = new Raylib6NativeTexture { Id = 99, Width = 128, Height = 64 };
        var texture = Texture.FromNative(native);
        await Assert.That(texture.Id).IsEqualTo(99u);
        await Assert.That(texture.Width).IsEqualTo(128);
        await Assert.That(texture.Height).IsEqualTo(64);
        await Assert.That(texture.IsValid).IsTrue();
    }

    [Test]
    public async Task IsValid_false_for_zero_id()
    {
        var texture = Texture.FromNative(default);
        await Assert.That(texture.IsValid).IsFalse();
    }
}

public sealed class RepoPathsTests
{
    [Test]
    public async Task Directory_helpers_resolve_under_repo_root()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        await Assert.That(RepoPaths.BindingsDir(repoRoot)).EndsWith("Novolis.Raylib.Bindings");
        await Assert.That(RepoPaths.RuntimeDir(repoRoot)).EndsWith("Novolis.Raylib.Runtime");
        await Assert.That(RepoPaths.InteropDir(repoRoot)).Contains("Interop");
        await Assert.That(RepoPaths.PipelineDir(repoRoot)).IsEqualTo(PipelinePaths.PipelineRaylibDir(repoRoot));
    }
}
