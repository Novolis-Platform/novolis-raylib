using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelinePathsTests
{
    [Test]
    public async Task FindRepoRoot_finds_slnx()
    {
        var root = PipelinePaths.FindRepoRoot();
        await Assert.That(File.Exists(Path.Combine(root, "Novolis.Raylib.slnx"))).IsTrue();
    }
}
