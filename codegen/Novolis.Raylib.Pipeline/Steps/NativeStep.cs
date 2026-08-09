namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class NativeStep : IPipelineStep
{
    public string Id => "step_02_native";

    public string Description => "Build native shims and copy outputs to step artifacts.";

    public IReadOnlyList<string> DependsOn => ["step_01_source"];

    public IReadOnlyList<string> InputPaths(PipelineContext context)
    {
        var list = new List<string> { PipelinePaths.VersionsJson(context.RepoRoot) };
        var step01 = StepResultWriter.TryRead(context.StepDir("step_01_source"));
        if (step01 is not null)
        {
            foreach (var key in step01.Inputs.Keys)
                list.Add(key);
        }

        return list;
    }

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
        NativeShimCatalog.ArtifactPaths(context.RepoRoot);

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var repoRoot = context.RepoRoot;
        if (OperatingSystem.IsWindows())
            await EnsureRaylibDefAsync(context, cancellationToken);

        foreach (var nativeDir in NativeShimCatalog.NativeProjectDirs(repoRoot))
        {
            if (!Directory.Exists(nativeDir))
                continue;

            var buildDir = Path.Combine(nativeDir, "build");
            Directory.CreateDirectory(buildDir);
            var raylibNativeDir = OperatingSystem.IsWindows()
                ? PipelinePaths.RaylibPrebuiltWinDir(repoRoot)
                : OperatingSystem.IsLinux()
                    ? PipelinePaths.RaylibPrebuiltLinuxDir(repoRoot)
                    : PipelinePaths.RaylibPrebuiltOsxDir(repoRoot);
            var configureArgs = $"-S \"{nativeDir}\" -B \"{buildDir}\" -DRAYLIB_NATIVE_DIR=\"{raylibNativeDir}\" -DRAYLIB_HEADER_DIR=\"{PipelinePaths.RaylibIncludeDir(repoRoot)}\"";
            var configureCode = await ProcessRunner.RunAsync(context, "cmake", configureArgs, repoRoot, cancellationToken);
            if (configureCode != 0)
            {
                try { Directory.Delete(buildDir, recursive: true); } catch { /* ignore */ }
                Directory.CreateDirectory(buildDir);
                configureCode = await ProcessRunner.RunAsync(context, "cmake", configureArgs, repoRoot, cancellationToken);
                if (configureCode != 0)
                    throw new InvalidOperationException($"cmake configure failed for {nativeDir} (exit {configureCode})");
            }

            var buildCode = await ProcessRunner.RunAsync(
                context,
                "cmake",
                $"--build \"{buildDir}\" --config Release",
                repoRoot,
                cancellationToken);
            if (buildCode != 0)
                throw new InvalidOperationException($"cmake build failed for {nativeDir} (exit {buildCode})");
        }

        var artifactsDir = PipelinePaths.NativeArtifactsDir(repoRoot);
        Directory.CreateDirectory(artifactsDir);
        foreach (var (source, destName) in NativeShimCatalog.CopyMap(repoRoot))
        {
            if (!File.Exists(source))
            {
                await context.Log.WriteLineAsync($"WARN: missing shim output {source}");
                continue;
            }

            var dest = Path.Combine(artifactsDir, destName);
            File.Copy(source, dest, overwrite: true);
            await context.Log.WriteLineAsync($"Copied {source} -> {dest}");
        }

        await StageDesktopRuntimesAsync(context, cancellationToken);

        var stepDir = context.StepDir(Id);
        var outputs = StepFileFingerprint.DescribeOutputs(ExpectedOutputPaths(context), repoRoot, stepDir);
        return new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), repoRoot),
            Outputs = outputs,
        };
    }

    private static async Task StageDesktopRuntimesAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var repoRoot = context.RepoRoot;

        // Host RID shims + prebuilt raylib → checked-in Native/runtimes (packaged by CI).
        if (OperatingSystem.IsWindows())
        {
            var win = PipelinePaths.RaylibNativeWinX64Dir(repoRoot);
            Directory.CreateDirectory(win);
            CopyIfExists(Path.Combine(PipelinePaths.RaylibPrebuiltWinDir(repoRoot), "raylib.dll"), Path.Combine(win, "raylib.dll"), context);
            CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "novolis_raylib_trace.dll"), Path.Combine(win, "novolis_raylib_trace.dll"), context);
            CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "novolis_imgui.dll"), Path.Combine(win, "novolis_imgui.dll"), context);
        }
        else if (OperatingSystem.IsLinux())
        {
            await StageLinuxFromHostBuildAsync(context);
        }
        else if (OperatingSystem.IsMacOS())
        {
            var osx = PipelinePaths.RaylibNativeOsxX64Dir(repoRoot);
            Directory.CreateDirectory(osx);
            CopyIfExists(Path.Combine(PipelinePaths.RaylibPrebuiltOsxDir(repoRoot), "libraylib.dylib"), Path.Combine(osx, "libraylib.dylib"), context);
            CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "libnovolis_raylib_trace.dylib"), Path.Combine(osx, "libnovolis_raylib_trace.dylib"), context);
            CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "libnovolis_imgui.dylib"), Path.Combine(osx, "libnovolis_imgui.dylib"), context);
        }

        // Always stage libraylib.so when the linux prebuilt was fetched (even on Windows).
        var linuxSo = Path.Combine(PipelinePaths.RaylibPrebuiltLinuxDir(repoRoot), "libraylib.so");
        if (File.Exists(linuxSo))
        {
            var linuxDir = PipelinePaths.RaylibNativeLinuxX64Dir(repoRoot);
            Directory.CreateDirectory(linuxDir);
            File.Copy(linuxSo, Path.Combine(linuxDir, "libraylib.so"), overwrite: true);
            await context.Log.WriteLineAsync($"Staged {linuxSo} -> {linuxDir}");
        }

        // Windows maintainers: build linux shims via WSL so the nupkg is complete for Novolis OS.
        if (OperatingSystem.IsWindows())
            await TryBuildLinuxShimsViaWslAsync(context, cancellationToken);
    }

    private static async Task StageLinuxFromHostBuildAsync(PipelineContext context)
    {
        var repoRoot = context.RepoRoot;
        var linux = PipelinePaths.RaylibNativeLinuxX64Dir(repoRoot);
        Directory.CreateDirectory(linux);
        CopyIfExists(Path.Combine(PipelinePaths.RaylibPrebuiltLinuxDir(repoRoot), "libraylib.so"), Path.Combine(linux, "libraylib.so"), context);
        CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-platform"), "libnovolis_raylib_trace.so"), Path.Combine(linux, "libnovolis_raylib_trace.so"), context);
        CopyIfExists(Path.Combine(PipelinePaths.NativeShimOutDir(repoRoot, "raylib6-with-imgui"), "libnovolis_imgui.so"), Path.Combine(linux, "libnovolis_imgui.so"), context);
        await Task.CompletedTask;
    }

    private static async Task TryBuildLinuxShimsViaWslAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var script = PipelinePaths.BuildLinuxShimsScript(context.RepoRoot);
        if (!File.Exists(script))
        {
            await context.Log.WriteLineAsync($"WARN: missing {script}; linux shim .so files not built.");
            return;
        }

        // Normalize LF for bash.
        var text = await File.ReadAllTextAsync(script, cancellationToken);
        var lf = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        if (!string.Equals(text, lf, StringComparison.Ordinal))
            await File.WriteAllTextAsync(script, lf, cancellationToken);

        var wslCheck = await ProcessRunner.RunAsync(context, "wsl", "-e true", context.RepoRoot, cancellationToken);
        if (wslCheck != 0)
        {
            await context.Log.WriteLineAsync("WARN: WSL unavailable — staged libraylib.so only; build linux shims on Linux or install WSL.");
            return;
        }

        await context.Log.WriteLineAsync("Building linux-x64 shims via WSL...");
        var unixScript = script.Replace('\\', '/');
        if (unixScript.Length >= 2 && unixScript[1] == ':')
            unixScript = "/mnt/" + char.ToLowerInvariant(unixScript[0]) + unixScript[2..];

        var code = await ProcessRunner.RunAsync(
            context,
            "wsl",
            $"-e bash \"{unixScript}\"",
            context.RepoRoot,
            cancellationToken);
        if (code != 0)
            throw new InvalidOperationException($"build-linux-shims.sh failed via WSL (exit {code})");
    }

    private static void CopyIfExists(string source, string dest, PipelineContext context)
    {
        if (!File.Exists(source))
        {
            context.Log.WriteLine($"WARN: missing {source}");
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(source, dest, overwrite: true);
        context.Log.WriteLine($"Staged {source} -> {dest}");
    }

    private static async Task EnsureRaylibDefAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var repoRoot = context.RepoRoot;
        var rayguiDir = Path.Combine(PipelinePaths.NativeRoot(repoRoot), "raylib6-with-raygui");
        var defPath = Path.Combine(rayguiDir, "generated", "raylib.win-x64.def");
        var raylibDll = Path.Combine(PipelinePaths.RaylibPrebuiltWinDir(repoRoot), "raylib.dll");
        if (!File.Exists(raylibDll) || File.Exists(defPath))
            return;

        var gen = Path.Combine(PipelinePaths.PipelineRaylibDir(repoRoot), "generate-raylib-windows-def.cs");
        if (!File.Exists(gen))
            throw new InvalidOperationException($"Missing {gen}");

        Directory.CreateDirectory(Path.GetDirectoryName(defPath)!);
        var code = await ProcessRunner.RunAsync(
            context,
            "dotnet",
            $"run \"{gen}\" -- \"{raylibDll}\" \"{defPath}\"",
            repoRoot,
            cancellationToken);
        if (code != 0)
            throw new InvalidOperationException($"generate-raylib-windows-def failed (exit {code})");
    }
}

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
