using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class NativeShimCatalogTests
{
    [Test]
    public async Task NativeProjectDirs_lists_three_native_trees()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var dirs = NativeShimCatalog.NativeProjectDirs(repoRoot).ToList();
        await Assert.That(dirs.Count).IsEqualTo(3);
        await Assert.That(dirs.All(d => d.Contains("codegen", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    [Test]
    public async Task CopyMap_returns_platform_specific_artifact_names()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var map = NativeShimCatalog.CopyMap(repoRoot).ToList();
        await Assert.That(map.Count).IsEqualTo(3);
        if (OperatingSystem.IsWindows())
            await Assert.That(map[0].DestName).IsEqualTo("novolis_raylib_trace.dll");
        else if (OperatingSystem.IsLinux())
            await Assert.That(map[0].DestName).IsEqualTo("libnovolis_raylib_trace.so");
        else if (OperatingSystem.IsMacOS())
            await Assert.That(map[0].DestName).IsEqualTo("libnovolis_raylib_trace.dylib");
    }

    [Test]
    public async Task ArtifactPaths_aligns_with_copy_map_dest_names()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var artifactPaths = NativeShimCatalog.ArtifactPaths(repoRoot);
        var destNames = NativeShimCatalog.CopyMap(repoRoot).Select(p => p.DestName).ToList();
        await Assert.That(artifactPaths.Count).IsEqualTo(destNames.Count);
        foreach (var dest in destNames)
            await Assert.That(artifactPaths.Any(p => p.EndsWith(dest, StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}
