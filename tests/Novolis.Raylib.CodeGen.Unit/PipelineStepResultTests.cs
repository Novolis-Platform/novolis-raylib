using System.Text.Json;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class PipelineStepResultTests
{
    [Test]
    public async Task StepResultDocument_round_trips_json()
    {
        var doc = new StepResultDocument
        {
            StepId = "step_03_verify_manifest",
            Status = StepStatus.Succeeded,
            StartedUtc = DateTimeOffset.Parse("2026-05-18T12:00:00Z"),
            DurationMs = 42,
            Inputs = new Dictionary<string, string> { ["codegen/pipeline/raylib6/versions.json"] = "abc" },
            Outputs = [new StepOutputRecord { Path = "artifacts/raylib-6/include/raylib.h", Sha256 = "def", Bytes = 100 }],
        };

        var json = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        var roundTrip = JsonSerializer.Deserialize<StepResultDocument>(json);
        await Assert.That(roundTrip).IsNotNull();
        await Assert.That(roundTrip!.StepId).IsEqualTo("step_03_verify_manifest");
        await Assert.That(roundTrip.Status).IsEqualTo(StepStatus.Succeeded);
    }

    [Test]
    public async Task StepSkipEvaluator_skips_when_inputs_and_outputs_match()
    {
        var repoRoot = RepoTestPaths.TryRepositoryRoot()
                       ?? throw new InvalidOperationException("Could not resolve repository root.");
        var stepDir = PipelinePaths.StepDir(repoRoot, "step_03_verify_manifest");
        Directory.CreateDirectory(stepDir);

        var step = new Pipeline.Steps.VerifyManifestStep();
        var context = new PipelineContext
        {
            RepoRoot = repoRoot,
            Log = TextWriter.Null,
            Force = false,
        };

        var inputPaths = step.InputPaths(context);
        var inputs = StepFileFingerprint.HashFiles(inputPaths, repoRoot);
        var doc = new StepResultDocument
        {
            StepId = "step_03_verify_manifest",
            Status = StepStatus.Succeeded,
            Inputs = new Dictionary<string, string>(inputs, StringComparer.Ordinal),
            Outputs = [],
        };
        StepResultWriter.Write(stepDir, doc);

        var skip = StepSkipEvaluator.ShouldSkip(step, context, doc, out var reason);
        await Assert.That(skip).IsTrue();
        await Assert.That(reason).IsEqualTo("outputs up to date");
    }
}
