using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

[NotInParallel("codegen-emit")]
public sealed class RaylibCodegenPipelineGenerateTests
{
    [Test]
    public async Task GenerateBindingsOnly_emits_manifest_sha_headers()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");
        var pipeline = new RaylibCodegenPipeline(root);
        using var writer = new StringWriter();
        pipeline.GenerateBindingsOnly(writer);
        var output = writer.ToString();
        await Assert.That(output).Contains("emit: raylib interop");
        await Assert.That(output).Contains("emit: facades");
    }
}
