using System.Runtime.InteropServices;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class Utf8StringMarshallerTests
{
    [Test]
    public async Task ConvertToUnmanaged_round_trips_utf8()
    {
        var ptr = Utf8StringMarshaller.ConvertToUnmanaged("hello");
        try
        {
            await Assert.That(ptr).IsNotEqualTo(nint.Zero);
            var roundTrip = Marshal.PtrToStringUTF8(ptr);
            await Assert.That(roundTrip).IsEqualTo("hello");
        }
        finally
        {
            Utf8StringMarshaller.Free(ptr);
        }
    }

    [Test]
    public async Task ConvertToUnmanaged_null_returns_zero()
    {
        var ptr = Utf8StringMarshaller.ConvertToUnmanaged(null);
        await Assert.That(ptr).IsEqualTo(nint.Zero);
    }

    [Test]
    public async Task Free_zero_is_noop()
    {
        Utf8StringMarshaller.Free(0);
        await Task.CompletedTask;
    }
}

public sealed class RaylibManifestVerifierEdgeTests
{
    [Test]
    public async Task Verify_returns_2_when_manifest_missing()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-codegen-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(PipelinePaths.PipelineRaylibDir(tempRoot));
        var code = RaylibManifestVerifier.Verify(tempRoot);
        await Assert.That(code).IsEqualTo(2);
        Directory.Delete(tempRoot, recursive: true);
    }

    [Test]
    public async Task Verify_returns_3_when_imports_empty()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-codegen-unit", Guid.NewGuid().ToString("N"));
        var dir = PipelinePaths.PipelineRaylibDir(tempRoot);
        var artifacts = Path.Combine(dir, "steps", "step_01_source", "artifacts", "raylib-6", "include");
        Directory.CreateDirectory(artifacts);
        File.WriteAllText(Path.Combine(dir, "raylib-exports.manifest.json"), """{"imports":[]}""");
        File.WriteAllText(Path.Combine(artifacts, "raylib.h"), "/* header present */\n");
        var code = RaylibManifestVerifier.Verify(tempRoot);
        await Assert.That(code).IsEqualTo(3);
        Directory.Delete(tempRoot, recursive: true);
    }
}

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
