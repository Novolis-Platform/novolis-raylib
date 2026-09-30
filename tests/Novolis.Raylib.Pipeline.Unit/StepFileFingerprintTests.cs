using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;

namespace Novolis.Raylib.Pipeline.Unit;

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
