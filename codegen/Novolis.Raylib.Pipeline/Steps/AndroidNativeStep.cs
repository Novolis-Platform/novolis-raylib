using System.Text.Json;

namespace Novolis.Raylib.Pipeline.Steps;

/// <summary>
/// NDK-build raylib static for android-arm64 and thin NativeActivity host .so; stage into Native/runtimes.
/// </summary>
internal sealed class AndroidNativeStep : IPipelineStep
{
    public string Id => "step_02a_android";

    public string Description =>
        "NDK-build libraylib.a (android-arm64) + libnovolis_raylib_android.so; stage into Native/runtimes.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
    [
        PipelinePaths.VersionsJson(context.RepoRoot),
        Path.Combine(PipelinePaths.NativeRoot(context.RepoRoot), "raylib6-android-host", "main.c"),
        Path.Combine(PipelinePaths.NativeRoot(context.RepoRoot), "raylib6-android-host", "CMakeLists.txt"),
    ];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
    [
        Path.Combine(PipelinePaths.RaylibNativeAndroidArm64Dir(context.RepoRoot), "libraylib.a"),
        Path.Combine(PipelinePaths.RaylibNativeAndroidArm64Dir(context.RepoRoot), "libnovolis_raylib_android.so"),
    ];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var ndkHome = ResolveNdkHome();
        if (ndkHome is null)
        {
            throw new InvalidOperationException(
                "Android NDK not found. Set ANDROID_NDK_HOME / ANDROID_NDK_ROOT, or install under %LOCALAPPDATA%\\Android\\Sdk\\ndk\\.");
        }

        await context.Log.WriteLineAsync($"Using NDK: {ndkHome}");

        var versionsPath = PipelinePaths.VersionsJson(context.RepoRoot);
        using var versionsDoc = JsonDocument.Parse(await File.ReadAllTextAsync(versionsPath, cancellationToken));
        var tag = versionsDoc.RootElement.TryGetProperty("raylibGitTag", out var tagEl)
            ? tagEl.GetString() ?? "6.0"
            : "6.0";

        var artifacts = PipelinePaths.AndroidNativeArtifactsDir(context.RepoRoot);
        Directory.CreateDirectory(artifacts);

        var raylibSrc = Path.Combine(artifacts, "raylib");
        await EnsureRaylibSourceAsync(context, raylibSrc, tag, cancellationToken);

        var raylibBuild = Path.Combine(artifacts, "raylib-build-android-arm64");
        Directory.CreateDirectory(raylibBuild);

        var toolchain = Path.Combine(ndkHome, "build", "cmake", "android.toolchain.cmake");
        if (!File.Exists(toolchain))
            throw new InvalidOperationException($"Missing Android CMake toolchain: {toolchain}");

        var configureRaylib =
            $"-S \"{raylibSrc}\" -B \"{raylibBuild}\" -G Ninja " +
            $"-DCMAKE_TOOLCHAIN_FILE=\"{toolchain}\" " +
            "-DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-24 " +
            "-DPLATFORM=Android -DBUILD_SHARED_LIBS=OFF -DBUILD_EXAMPLES=OFF " +
            "-DCMAKE_BUILD_TYPE=Release";

        var cfgCode = await ProcessRunner.RunAsync(context, "cmake", configureRaylib, context.RepoRoot, cancellationToken);
        if (cfgCode != 0)
            throw new InvalidOperationException($"cmake configure failed for raylib Android (exit {cfgCode})");

        var buildCode = await ProcessRunner.RunAsync(
            context,
            "cmake",
            $"--build \"{raylibBuild}\" --config Release",
            context.RepoRoot,
            cancellationToken);
        if (buildCode != 0)
            throw new InvalidOperationException($"cmake build failed for raylib Android (exit {buildCode})");

        var libraylibA = Path.Combine(raylibBuild, "raylib", "libraylib.a");
        if (!File.Exists(libraylibA))
            throw new InvalidOperationException($"Expected static library missing: {libraylibA}");

        var hostDir = Path.Combine(PipelinePaths.NativeRoot(context.RepoRoot), "raylib6-android-host");
        var hostBuild = Path.Combine(hostDir, "build-android-arm64");
        Directory.CreateDirectory(hostBuild);

        var raylibInclude = Path.Combine(raylibSrc, "src");
        var configureHost =
            $"-S \"{hostDir}\" -B \"{hostBuild}\" -G Ninja " +
            $"-DCMAKE_TOOLCHAIN_FILE=\"{toolchain}\" " +
            "-DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-24 " +
            "-DCMAKE_BUILD_TYPE=Release " +
            $"-DRAYLIB_ANDROID_LIB=\"{libraylibA}\" " +
            $"-DRAYLIB_ANDROID_INCLUDE=\"{raylibInclude}\"";

        cfgCode = await ProcessRunner.RunAsync(context, "cmake", configureHost, context.RepoRoot, cancellationToken);
        if (cfgCode != 0)
            throw new InvalidOperationException($"cmake configure failed for android host (exit {cfgCode})");

        buildCode = await ProcessRunner.RunAsync(
            context,
            "cmake",
            $"--build \"{hostBuild}\" --config Release",
            context.RepoRoot,
            cancellationToken);
        if (buildCode != 0)
            throw new InvalidOperationException($"cmake build failed for android host (exit {buildCode})");

        var hostSo = Path.Combine(hostDir, "out", "libnovolis_raylib_android.so");
        if (!File.Exists(hostSo))
            throw new InvalidOperationException($"Expected host .so missing: {hostSo}");

        var stageDir = PipelinePaths.RaylibNativeAndroidArm64Dir(context.RepoRoot);
        Directory.CreateDirectory(stageDir);
        var stagedA = Path.Combine(stageDir, "libraylib.a");
        var stagedSo = Path.Combine(stageDir, "libnovolis_raylib_android.so");
        File.Copy(libraylibA, stagedA, overwrite: true);
        File.Copy(hostSo, stagedSo, overwrite: true);
        await context.Log.WriteLineAsync($"Staged {stagedA}");
        await context.Log.WriteLineAsync($"Staged {stagedSo}");

        var stepDir = context.StepDir(Id);
        var outputs = StepFileFingerprint.DescribeOutputs(ExpectedOutputPaths(context), context.RepoRoot, stepDir);
        return new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
            Outputs = outputs,
        };
    }

    private static string? ResolveNdkHome()
    {
        foreach (var key in new[] { "ANDROID_NDK_HOME", "ANDROID_NDK_ROOT" })
        {
            var v = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(v) && Directory.Exists(v))
                return v;
        }

        var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME")
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android",
                "Sdk");

        var ndkRoot = Path.Combine(sdk, "ndk");
        if (!Directory.Exists(ndkRoot))
            return null;

        // Prefer highest version / newest directory that has the toolchain file.
        foreach (var dir in Directory.GetDirectories(ndkRoot).OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(dir, "build", "cmake", "android.toolchain.cmake")))
                return dir;
        }

        return null;
    }

    private static async Task EnsureRaylibSourceAsync(
        PipelineContext context,
        string dest,
        string tag,
        CancellationToken cancellationToken)
    {
        if (File.Exists(Path.Combine(dest, "src", "raylib.h")))
        {
            await context.Log.WriteLineAsync($"raylib sources present at {dest}");
            return;
        }

        if (Directory.Exists(dest))
            Directory.Delete(dest, recursive: true);

        var url = "https://github.com/raysan5/raylib.git";
        await context.Log.WriteLineAsync($"Cloning {url} (tag {tag}) -> {dest}");
        var code = await ProcessRunner.RunAsync(
            context,
            "git",
            $"clone --depth 1 --branch {tag} \"{url}\" \"{dest}\"",
            context.RepoRoot,
            cancellationToken);
        if (code != 0 || !File.Exists(Path.Combine(dest, "src", "raylib.h")))
            throw new InvalidOperationException($"Failed to clone raylib {tag} into {dest} (exit {code})");
    }
}
