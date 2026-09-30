using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class StepSkipEvaluatorTests
{
    [Test]
    public async Task Force_never_skips()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var layout = new RaylibPipelineLayout(repoRoot);
        var step = new VerifyManifestStep();
        var context = new PipelineContext { Layout = layout, Log = TextWriter.Null, Force = true };
        var previous = new StepResultDocument { StepId = step.Id, Status = StepStatus.Succeeded, Inputs = [] };
        var skip = StepSkipEvaluator.ShouldSkip(step, context, previous, out _);
        await Assert.That(skip).IsFalse();
    }

    [Test]
    public async Task Missing_previous_result_does_not_skip()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var layout = new RaylibPipelineLayout(repoRoot);
        var step = new VerifyManifestStep();
        var context = new PipelineContext { Layout = layout, Log = TextWriter.Null, Force = false };
        var skip = StepSkipEvaluator.ShouldSkip(step, context, previous: null, out _);
        await Assert.That(skip).IsFalse();
    }

    [Test]
    public async Task Input_hash_mismatch_does_not_skip()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var layout = new RaylibPipelineLayout(repoRoot);
        var step = new VerifyManifestStep();
        var context = new PipelineContext { Layout = layout, Log = TextWriter.Null, Force = false };
        var previous = new StepResultDocument
        {
            StepId = step.Id,
            Status = StepStatus.Succeeded,
            Inputs = new Dictionary<string, string> { ["stale"] = "hash" },
        };
        var skip = StepSkipEvaluator.ShouldSkip(step, context, previous, out _);
        await Assert.That(skip).IsFalse();
    }
}
