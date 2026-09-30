using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class RaylibManifestVerifierTests
{
    [Test]
    public async Task Verify_skips_when_header_missing()
    {
        const string repoRoot = @"C:\novolis\pipeline-test";
        var env = PipelineTestEnvironment.CreateMock(repoRoot, new Dictionary<string, string>());
        var code = RaylibManifestVerifier.Verify(env, PipelineTestEnvironment.Manifests(
            PipelineTestEnvironment.Interop(new InteropImportSpec("InitWindow", NativeSignature.Create(NativeType.Void)))));
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Verify_fails_when_symbol_missing_from_header()
    {
        const string repoRoot = @"C:\novolis\pipeline-test";
        var env = PipelineTestEnvironment.CreateMock(
            repoRoot,
            new Dictionary<string, string>
            {
                [PipelineTestEnvironment.RaylibHeaderRelativePath] =
                    "RLAPI void InitWindow(int w, int h, const char* t);\n",
            });
        var code = RaylibManifestVerifier.Verify(
            env,
            PipelineTestEnvironment.Manifests(
                PipelineTestEnvironment.Interop(new InteropImportSpec("MissingSymbol", NativeSignature.Create(NativeType.Void)))));
        await Assert.That(code).IsEqualTo(4);
    }

    [Test]
    public async Task Verify_succeeds_when_symbol_present()
    {
        const string repoRoot = @"C:\novolis\pipeline-test";
        var env = PipelineTestEnvironment.CreateMock(
            repoRoot,
            new Dictionary<string, string>
            {
                [PipelineTestEnvironment.RaylibHeaderRelativePath] =
                    "RLAPI void InitWindow(int w, int h, const char* t);\n",
            });
        var code = RaylibManifestVerifier.Verify(
            env,
            PipelineTestEnvironment.Manifests(
                PipelineTestEnvironment.Interop(new InteropImportSpec("InitWindow", NativeSignature.Create(NativeType.Void)))));
        await Assert.That(code).IsEqualTo(0);
    }
}
