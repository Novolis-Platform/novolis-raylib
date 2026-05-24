using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineProfilesTests
{
    [Test]
    public async Task Resolve_generate_profile_includes_verify_and_codegen()
    {
        var steps = PipelineProfiles.Resolve("generate");
        await Assert.That(steps.Count).IsEqualTo(2);
        await Assert.That(steps[0]).IsEqualTo("step_03_verify_manifest");
        await Assert.That(steps[1]).IsEqualTo("step_06_codegen");
    }

    [Test]
    public async Task Resolve_single_step_id()
    {
        var steps = PipelineProfiles.Resolve("step_08_build");
        await Assert.That(steps.Count).IsEqualTo(1);
        await Assert.That(steps[0]).IsEqualTo("step_08_build");
    }

    [Test]
    public async Task Explain_returns_description_for_known_step()
    {
        var text = PipelineProfiles.Explain("step_06_codegen");
        await Assert.That(text).IsNotNull();
        await Assert.That(text!).Contains("g.cs");
    }
}

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

public sealed class RaylibManifestVerifierTests
{
    [Test]
    public async Task Verify_skips_when_header_missing()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        var pipelineDir = PipelinePaths.PipelineRaylibDir(tempRoot);
        Directory.CreateDirectory(pipelineDir);
        File.WriteAllText(
            Path.Combine(pipelineDir, "raylib-exports.manifest.json"),
            """{"imports":[{"name":"InitWindow"}]}""");
        var code = RaylibManifestVerifier.Verify(tempRoot);
        await Assert.That(code).IsEqualTo(0);
        Directory.Delete(tempRoot, recursive: true);
    }

    [Test]
    public async Task Verify_fails_when_symbol_missing_from_header()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        var pipelineDir = PipelinePaths.PipelineRaylibDir(tempRoot);
        var artifacts = Path.Combine(pipelineDir, "steps", "step_01_source", "artifacts", "raylib-6", "include");
        Directory.CreateDirectory(artifacts);
        File.WriteAllText(
            Path.Combine(pipelineDir, "raylib-exports.manifest.json"),
            """{"imports":[{"name":"MissingSymbol"}]}""");
        File.WriteAllText(Path.Combine(artifacts, "raylib.h"), "RLAPI void InitWindow(int w, int h, const char* t);\n");
        var code = RaylibManifestVerifier.Verify(tempRoot);
        await Assert.That(code).IsEqualTo(4);
        Directory.Delete(tempRoot, recursive: true);
    }

    [Test]
    public async Task Verify_succeeds_when_symbol_present()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        var pipelineDir = PipelinePaths.PipelineRaylibDir(tempRoot);
        var artifacts = Path.Combine(pipelineDir, "steps", "step_01_source", "artifacts", "raylib-6", "include");
        Directory.CreateDirectory(artifacts);
        File.WriteAllText(
            Path.Combine(pipelineDir, "raylib-exports.manifest.json"),
            """{"imports":[{"name":"InitWindow"}]}""");
        File.WriteAllText(Path.Combine(artifacts, "raylib.h"), "RLAPI void InitWindow(int w, int h, const char* t);\n");
        var code = RaylibManifestVerifier.Verify(tempRoot);
        await Assert.That(code).IsEqualTo(0);
        Directory.Delete(tempRoot, recursive: true);
    }
}
