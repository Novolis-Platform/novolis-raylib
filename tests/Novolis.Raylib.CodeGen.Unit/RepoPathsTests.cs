using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RepoPathsTests
{
    [Test]
    public async Task Directory_helpers_resolve_under_repo_root()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        await Assert.That(RepoPaths.BindingsDir(repoRoot)).EndsWith("Novolis.Raylib.Bindings");
        await Assert.That(RepoPaths.RuntimeDir(repoRoot)).EndsWith("Novolis.Raylib.Runtime");
        await Assert.That(RepoPaths.InteropDir(repoRoot)).Contains("Interop");
        await Assert.That(RepoPaths.PipelineDir(repoRoot)).IsEqualTo(PipelinePaths.PipelineRaylibDir(repoRoot));
    }
}
