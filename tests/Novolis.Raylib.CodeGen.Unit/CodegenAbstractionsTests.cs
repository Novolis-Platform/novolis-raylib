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
