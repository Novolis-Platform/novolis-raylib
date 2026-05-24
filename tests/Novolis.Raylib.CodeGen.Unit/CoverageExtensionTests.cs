using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
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
    public async Task Verify_returns_3_when_imports_empty()
    {
        const string repoRoot = @"C:\novolis\raylib-test";
        var env = CodegenTestEnvironment.CreateMock(
            repoRoot,
            new Dictionary<string, string>
            {
                [CodegenTestEnvironment.RaylibHeaderRelativePath] = "/* header present */\n",
            });
        var manifests = CodegenTestEnvironment.Manifests(CodegenTestEnvironment.InteropFragment());
        var code = RaylibManifestVerifier.Verify(env, manifests);
        await Assert.That(code).IsEqualTo(3);
    }

    [Test]
    public async Task Verify_returns_4_when_symbol_missing_from_header()
    {
        const string repoRoot = @"C:\novolis\raylib-test";
        var env = CodegenTestEnvironment.CreateMock(
            repoRoot,
            new Dictionary<string, string>
            {
                [CodegenTestEnvironment.RaylibHeaderRelativePath] =
                    "RLAPI void InitWindow(int w, int h, const char* t);\n",
            });
        var manifests = CodegenTestEnvironment.Manifests(
            CodegenTestEnvironment.InteropFragment(new InteropImportSpec("MissingSymbol", "void_v")));
        var code = RaylibManifestVerifier.Verify(env, manifests);
        await Assert.That(code).IsEqualTo(4);
    }
}

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
