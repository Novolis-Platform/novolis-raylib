using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class CodegenOutputCatalogTests
{
    [Test]
    public async Task AllGeneratedFiles_includes_bindings_and_runtime_gcs()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var files = CodegenOutputCatalog.AllGeneratedFiles(repoRoot);
        await Assert.That(files.Count).IsGreaterThan(0);
        await Assert.That(files.Any(f => f.EndsWith("Raylib6Native.g.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(files.Any(f => f.EndsWith("Graphics.g.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}
