using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineRunnerTests
{
    [Test]
    public async Task RunStepAsync_writes_result_json_for_fake_step()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(Path.Combine(tempRoot, "Directory.Packages.props"), "<Project/>");

        var stepId = "step_fake_success";
        var stepDir = PipelinePaths.StepDir(tempRoot, stepId);
        Directory.CreateDirectory(stepDir);

        var layout = new RaylibPipelineLayout(tempRoot);
        var runner = new PipelineRunner([new FakeSuccessStep(stepId)], layout);
        var exit = await runner.RunStepAsync(stepId, force: true);
        await Assert.That(exit).IsEqualTo(0);

        var resultPath = Path.Combine(stepDir, "result.json");
        await Assert.That(File.Exists(resultPath)).IsTrue();
        var doc = StepResultWriter.TryRead(stepDir);
        await Assert.That(doc).IsNotNull();
        await Assert.That(doc!.Status).IsEqualTo(StepStatus.Succeeded);

        Directory.Delete(tempRoot, recursive: true);
    }

    [Test]
    public async Task RunStepAsync_returns_nonzero_for_failed_step()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(Path.Combine(tempRoot, "Directory.Packages.props"), "<Project/>");

        var failId = "step_fake_fail";
        var layout = new RaylibPipelineLayout(tempRoot);
        var runner = new PipelineRunner([new FakeFailStep(failId)], layout);

        var exit = await runner.RunStepAsync(failId, force: true);
        await Assert.That(exit).IsEqualTo(1);
        var doc = StepResultWriter.TryRead(PipelinePaths.StepDir(tempRoot, failId));
        await Assert.That(doc!.Status).IsEqualTo(StepStatus.Failed);

        Directory.Delete(tempRoot, recursive: true);
    }

    private sealed class FakeSuccessStep(string id) : IPipelineStep
    {
        public string Id => id;
        public string Description => "fake success";
        public IReadOnlyList<string> DependsOn => [];
        public IReadOnlyList<string> InputPaths(PipelineContext context) => [];
        public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];
        public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new StepExecutionResult { Status = StepStatus.Succeeded });
    }

    private sealed class FakeFailStep(string id) : IPipelineStep
    {
        public string Id => id;
        public string Description => "fake fail";
        public IReadOnlyList<string> DependsOn => [];
        public IReadOnlyList<string> InputPaths(PipelineContext context) => [];
        public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];
        public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = "boom" },
            });
    }
}
