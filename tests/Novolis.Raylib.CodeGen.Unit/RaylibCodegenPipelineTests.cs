using System.Security.Cryptography;
using System.Text;
using Novolis.Raylib.CodeGen;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibCodegenPipelineTests
{
    [Test]
    public async Task Raylib_interop_emitter_manifest_sha_matches_committed_g_cs()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raylib-exports.manifest.json");
        var manifestBytes = await File.ReadAllBytesAsync(manifestPath);
        var manifestSha256 = Convert.ToHexString(SHA256.HashData(manifestBytes)).ToLowerInvariant();

        var committedPath = Path.Combine(
            root,
            "src", "Novolis.Raylib.Bindings", "Interop",
            "Raylib6Native.g.cs");
        var committed = await File.ReadAllTextAsync(committedPath);
        var committedShaLine = committed.Split('\n').First(l => l.Contains("// ManifestSha256:", StringComparison.Ordinal));
        await Assert.That(committedShaLine).Contains(manifestSha256);

        var emitted = RaylibInteropEmitter.Emit(manifestPath, manifestBytes, manifestSha256);
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
        await Assert.That(graphics).Contains("RaylibFrameCaptureHub.Notify()");
    }
}
