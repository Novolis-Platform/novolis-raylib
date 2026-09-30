using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

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
            CodegenTestEnvironment.InteropFragment(new InteropImportSpec("MissingSymbol", NativeSignature.Create(NativeType.Void))));
        var code = RaylibManifestVerifier.Verify(env, manifests);
        await Assert.That(code).IsEqualTo(4);
    }
}
