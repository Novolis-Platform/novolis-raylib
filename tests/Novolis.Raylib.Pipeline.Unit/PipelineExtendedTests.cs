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
