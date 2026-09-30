using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

[NotInParallel("pipeline-repo")]
public sealed class PipelineStepExecutionTests
{
    [Test]
    public async Task VerifyManifestStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new VerifyManifestStep();
        var log = new StringWriter();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = log,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
        await Assert.That(result.Inputs.Count).IsGreaterThan(0);
        await Assert.That(log.ToString()).Contains("verify-raylib-manifest: OK");
    }

    [Test]
    public async Task VerifyDocsStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new VerifyDocsStep();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = TextWriter.Null,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
    }

    [Test]
    public async Task CodegenStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new CodegenStep();
        var log = new StringWriter();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = log,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
        await Assert.That(result.Outputs.Count).IsGreaterThan(0);
    }
}
