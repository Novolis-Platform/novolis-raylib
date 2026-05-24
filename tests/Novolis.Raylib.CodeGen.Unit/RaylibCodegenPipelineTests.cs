using System.Security.Cryptography;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibCodegenPipelineTests
{
    [Test]
    public async Task Raylib_interop_emitter_manifest_sha_matches_committed_g_cs()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");
        var manifestSha256 = interop.Sha256Hex();

        var committedPath = Path.Combine(
            root,
            "src", "Novolis.Raylib.Bindings", "Interop",
            "Raylib6Native.g.cs");
        var committed = await File.ReadAllTextAsync(committedPath);
        var committedShaLine = committed.Split('\n').First(l => l.Contains("// ManifestSha256:", StringComparison.Ordinal));
        await Assert.That(committedShaLine).Contains(manifestSha256);

        var policy = RaylibManifestMapping.ToPolicy(interop.Policy);
        var emitted = RaylibInteropEmitter.Emit(interop, manifestSha256, policy);
        await Assert.That(emitted).Contains($"// ManifestSha256: {manifestSha256}");
        await Assert.That(emitted).Contains("internal static partial void BeginDrawing();");
    }

    [Test]
    public async Task Committed_Graphics_facade_includes_EndDrawing_debug_notify()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var graphics = await File.ReadAllTextAsync(Path.Combine(
            root,
            "src",
            "Novolis.Raylib.Runtime",
            "Rendering",
            "Graphics.g.cs"));

        await Assert.That(graphics).Contains("Raylib6Native.EndDrawing()");
        await Assert.That(graphics).Contains("RaylibDebugFrameHooks.NotifyAfterEndDrawing()");
        await Assert.That(graphics).Contains("RaylibPresentationHooks.Notify()");
    }
}
