using System.Numerics;
using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibManifestSuggesterUnitTests
{
    [Test]
    public async Task Suggest_returns_2_when_header_missing()
    {
        const string repoRoot = @"C:\novolis\raylib-test";
        var env = CodegenTestEnvironment.CreateMock(repoRoot, new Dictionary<string, string>());
        var manifests = CodegenTestEnvironment.Manifests(
            CodegenTestEnvironment.InteropFragment(new InteropImportSpec("InitWindow", NativeSignature.Create(NativeType.Void))));
        var code = RaylibManifestSuggester.Suggest(env, manifests);
        await Assert.That(code).IsEqualTo(2);
    }

    [Test]
    public async Task Suggest_reports_missing_symbols()
    {
        const string repoRoot = @"C:\novolis\raylib-test";
        var env = CodegenTestEnvironment.CreateMock(
            repoRoot,
            new Dictionary<string, string>
            {
                [CodegenTestEnvironment.RaylibHeaderRelativePath] =
                    "RLAPI void InitWindow(int w, int h, const char* t);\nRLAPI void CloseWindow(void);\n",
            });
        var manifests = CodegenTestEnvironment.Manifests(
            CodegenTestEnvironment.InteropFragment(new InteropImportSpec("InitWindow", NativeSignature.Create(NativeType.Void))));
        var code = RaylibManifestSuggester.Suggest(env, manifests);
        await Assert.That(code).IsEqualTo(0);
    }
}
