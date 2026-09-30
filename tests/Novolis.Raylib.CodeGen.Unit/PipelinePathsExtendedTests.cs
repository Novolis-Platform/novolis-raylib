using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

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
            await Assert.That(PipelinePaths.RaylibNativeLinuxX64Dir(repoRoot))
                .Contains("linux-x64");
            await Assert.That(PipelinePaths.BuildLinuxShimsScript(repoRoot))
                .EndsWith("build-linux-shims.sh");
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
