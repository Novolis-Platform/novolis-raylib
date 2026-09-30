namespace Novolis.Raylib.Pipeline.Steps;

internal static class NativeShimCatalog
{
    public static IEnumerable<string> NativeProjectDirs(string repoRoot) =>
    [
        Path.Combine(PipelinePaths.NativeRoot(repoRoot), "raylib6-with-raygui"),
        Path.Combine(PipelinePaths.NativeRoot(repoRoot), "raylib6-platform"),
        Path.Combine(PipelinePaths.NativeRoot(repoRoot), "raylib6-with-imgui"),
    ];

    public static IReadOnlyList<string> ArtifactPaths(string repoRoot)
    {
        var dir = PipelinePaths.NativeArtifactsDir(repoRoot);
        return CopyMap(repoRoot).Select(pair => Path.Combine(dir, pair.DestName)).ToList();
    }

    public static IEnumerable<(string Source, string DestName)> CopyMap(string repoRoot)
    {
        if (OperatingSystem.IsWindows())
        {
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "novolis_raylib_trace.dll"), "novolis_raylib_trace.dll");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "novolis_imgui.dll"), "novolis_imgui.dll");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-raygui"), "novolis_raygui.dll"), "novolis_raygui.dll");
        }
        else if (OperatingSystem.IsLinux())
        {
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "libnovolis_raylib_trace.so"), "libnovolis_raylib_trace.so");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "libnovolis_imgui.so"), "libnovolis_imgui.so");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-raygui"), "libnovolis_raygui.so"), "libnovolis_raygui.so");
        }
        else if (OperatingSystem.IsMacOS())
        {
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "libnovolis_raylib_trace.dylib"), "libnovolis_raylib_trace.dylib");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "libnovolis_imgui.dylib"), "libnovolis_imgui.dylib");
            yield return (Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-raygui"), "libnovolis_raygui.dylib"), "libnovolis_raygui.dylib");
        }
    }
}
