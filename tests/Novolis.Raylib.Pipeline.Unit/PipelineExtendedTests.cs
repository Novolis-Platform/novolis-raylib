using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class StepResultWriterTests
{
    [Test]
    public async Task Write_and_TryRead_round_trip()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var doc = new StepResultDocument
        {
            StepId = "step_test",
            Status = StepStatus.Succeeded,
            DurationMs = 10,
            Inputs = new Dictionary<string, string> { ["a.txt"] = "abc" },
        };
        StepResultWriter.Write(tempDir, doc);
        var read = StepResultWriter.TryRead(tempDir);
        await Assert.That(read).IsNotNull();
        await Assert.That(read!.StepId).IsEqualTo("step_test");
        Directory.Delete(tempDir, recursive: true);
    }

    [Test]
    public async Task TryRead_returns_null_when_missing()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        await Assert.That(StepResultWriter.TryRead(tempDir)).IsNull();
    }
}

public sealed class StepFileFingerprintTests
{
    [Test]
    public async Task HashFiles_and_Sha256Hex_are_stable()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var file = Path.Combine(tempRoot, "input.txt");
        File.WriteAllText(file, "hello");
        var hash = StepFileFingerprint.Sha256Hex(file);
        var map = StepFileFingerprint.HashFiles([file], tempRoot);
        await Assert.That(map.Values).Contains(hash);
        var outputs = StepFileFingerprint.DescribeOutputs([file], tempRoot);
        await Assert.That(outputs.Count).IsEqualTo(1);
        await Assert.That(outputs[0].Bytes.GetValueOrDefault()).IsGreaterThan(0L);
        Directory.Delete(tempRoot, recursive: true);
    }
}

public sealed class PipelineProfilesExtendedTests
{
    [Test]
    public async Task Resolve_all_known_profiles()
    {
        foreach (var profile in new[] { "maintainer", "generate", "ci-codegen", "agent-verify" })
        {
            var steps = PipelineProfiles.Resolve(profile);
            await Assert.That(steps.Count).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task Resolve_unknown_profile_throws()
    {
        var threw = false;
        try
        {
            PipelineProfiles.Resolve("not-a-profile");
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Explain_unknown_step_returns_null()
    {
        await Assert.That(PipelineProfiles.Explain("step_unknown")).IsNull();
    }
}

public sealed class PipelinePathsTests
{
    [Test]
    public async Task FindRepoRoot_finds_slnx()
    {
        var root = PipelinePaths.FindRepoRoot();
        await Assert.That(File.Exists(Path.Combine(root, "Novolis.Raylib.slnx"))).IsTrue();
    }
}
