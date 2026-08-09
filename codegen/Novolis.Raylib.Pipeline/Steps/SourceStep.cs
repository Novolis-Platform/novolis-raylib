using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class SourceStep : IPipelineStep
{
    public string Id => "step_01_source";

    public string Description => "Fetch raylib prebuilts, raygui header, and raylib-cimgui sources.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        [PipelinePaths.VersionsJson(context.RepoRoot)];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context)
    {
        var list = new List<string>
        {
            PipelinePaths.RaylibHeaderPath(context.RepoRoot),
            PipelinePaths.RayguiHeaderPath(context.RepoRoot),
            Path.Combine(PipelinePaths.RaylibPrebuiltWinDir(context.RepoRoot), "raylib.dll"),
            Path.Combine(PipelinePaths.RaylibPrebuiltLinuxDir(context.RepoRoot), "libraylib.so"),
        };

        var cimgui = Path.Combine(
            PipelinePaths.RaylibCimguiRoot(context.RepoRoot),
            "cimgui-1.92.1-docking",
            "cimgui.h");
        list.Add(cimgui);
        return list;
    }

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var versionsPath = PipelinePaths.VersionsJson(context.RepoRoot);
        var json = JsonSerializer.Deserialize<VersionsManifest>(
            await File.ReadAllTextAsync(versionsPath, cancellationToken),
            JsonSerializerOptions.Web)!;

        var artifacts = PipelinePaths.SourceArtifactsDir(context.RepoRoot);
        Directory.CreateDirectory(artifacts);

        MirrorLegacyVendorTrees(context);

        var raylibRoot = PipelinePaths.RaylibRoot(context.RepoRoot);
        var rayguiRoot = PipelinePaths.RayguiRoot(context.RepoRoot);
        var cimguiRoot = PipelinePaths.RaylibCimguiRoot(context.RepoRoot);
        Directory.CreateDirectory(raylibRoot);
        Directory.CreateDirectory(rayguiRoot);

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

        // Fetch desktop prebuilts for every RID we package — not only the host OS —
        // so Windows maintainers can stage linux-x64 natives for Novolis OS / RID publish.
        if (json.Prebuilt is null)
            throw new InvalidOperationException("versions.json missing prebuilt map");

        if (json.Prebuilt.TryGetValue("windows-x64", out var winUrl) && !string.IsNullOrEmpty(winUrl))
        {
            var zipPath = Path.Combine(raylibRoot, "download_win.zip");
            await context.Log.WriteLineAsync($"Downloading {winUrl}");
            await DownloadToFileAsync(http, winUrl, zipPath, cancellationToken);
            ExtractZipDllAndIncludes(zipPath, raylibRoot);
            File.Delete(zipPath);
            await context.Log.WriteLineAsync($"Extracted windows-x64 raylib prebuilt under {raylibRoot}");
        }
        else
            throw new InvalidOperationException("versions.json missing prebuilt.windows-x64");

        if (json.Prebuilt.TryGetValue("linux-x64", out var linuxUrl) && !string.IsNullOrEmpty(linuxUrl))
        {
            var tarPath = Path.Combine(raylibRoot, "download_linux.tar.gz");
            await context.Log.WriteLineAsync($"Downloading {linuxUrl}");
            await DownloadToFileAsync(http, linuxUrl, tarPath, cancellationToken);
            ExtractLinuxTarGz(tarPath, raylibRoot);
            File.Delete(tarPath);
            await context.Log.WriteLineAsync($"Extracted linux-x64 raylib prebuilt under {raylibRoot}");
        }
        else
            throw new InvalidOperationException("versions.json missing prebuilt.linux-x64");

        if (json.RayguiHeaderUrl is { } rayguiUrl)
        {
            var dest = PipelinePaths.RayguiHeaderPath(context.RepoRoot);
            await context.Log.WriteLineAsync($"Downloading {rayguiUrl}");
            await DownloadToFileAsync(http, rayguiUrl, dest, cancellationToken);
            FixRayguiIncludesIfNeeded(dest);
            await context.Log.WriteLineAsync($"Wrote {dest}");
        }

        if (json.RaylibCimguiRepoUrl is { } raylibCimguiRepo)
        {
            await EnsureGitCloneAsync(
                context,
                raylibCimguiRepo,
                cimguiRoot,
                json.RaylibCimguiGitRef ?? "1.92.1-docking",
                recursive: true,
                cancellationToken);
            var bundledCimgui = Path.Combine(cimguiRoot, json.CimguiBundledSubdir ?? "cimgui-1.92.1-docking");
            if (!File.Exists(Path.Combine(bundledCimgui, "cimgui.h")))
                throw new InvalidOperationException($"Missing bundled cimgui at {bundledCimgui}.");
            await context.Log.WriteLineAsync($"raylib-cimgui ready at {cimguiRoot}");
        }

        var stepDir = context.StepDir(Id);
        var outputs = StepFileFingerprint.DescribeOutputs(ExpectedOutputPaths(context), context.RepoRoot, stepDir);
        return new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
            Outputs = outputs,
        };
    }

    private static void MirrorLegacyVendorTrees(PipelineContext context)
    {
        var repoRoot = context.RepoRoot;
        var pairs = new[]
        {
            (Path.Combine(PipelinePaths.VendorRoot(repoRoot), "raylib-6"), PipelinePaths.RaylibRoot(repoRoot)),
            (Path.Combine(PipelinePaths.VendorRoot(repoRoot), "raygui-6"), PipelinePaths.RayguiRoot(repoRoot)),
            (Path.Combine(PipelinePaths.VendorRoot(repoRoot), "raylib-cimgui"), PipelinePaths.RaylibCimguiRoot(repoRoot)),
        };

        foreach (var (source, dest) in pairs)
        {
            if (!Directory.Exists(source) || Directory.Exists(dest) && Directory.EnumerateFileSystemEntries(dest).Any())
                continue;

            context.Log.WriteLine($"Mirroring legacy {source} -> {dest}");
            CopyDirectory(source, dest);
        }
    }

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, dest, StringComparison.OrdinalIgnoreCase));

        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = file.Replace(source, dest, StringComparison.OrdinalIgnoreCase);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target, overwrite: true);
        }
    }

    private static void ExtractLinuxTarGz(string tarGzPath, string destRoot)
    {
        var prebuiltRoot = Path.Combine(destRoot, "prebuilt", "linux-x64");
        var includeRoot = Path.Combine(destRoot, "include");
        Directory.CreateDirectory(prebuiltRoot);
        Directory.CreateDirectory(includeRoot);

        string? extractedSo = null;

        using var fileStream = File.OpenRead(tarGzPath);
        using var gzip = new System.IO.Compression.GZipStream(fileStream, System.IO.Compression.CompressionMode.Decompress);
        using var reader = new System.Formats.Tar.TarReader(gzip);

        while (reader.GetNextEntry() is { } entry)
        {
            var name = entry.Name.Replace('\\', '/');
            var fileName = Path.GetFileName(name);

            // Official linux tarball ships libraylib.so as a symlink; extract the real SONAME object.
            if (entry.EntryType is System.Formats.Tar.TarEntryType.RegularFile or System.Formats.Tar.TarEntryType.V7RegularFile)
            {
                if (fileName.StartsWith("libraylib.so", StringComparison.OrdinalIgnoreCase))
                {
                    var dest = Path.Combine(prebuiltRoot, fileName);
                    entry.ExtractToFile(dest, overwrite: true);
                    // Prefer the fully versioned .so (real ELF); fall back to unversioned if present as a file.
                    if (fileName.Contains(".so.", StringComparison.Ordinal) ||
                        extractedSo is null ||
                        fileName.Equals("libraylib.so", StringComparison.OrdinalIgnoreCase))
                    {
                        if (fileName.Contains(".so.", StringComparison.Ordinal) || extractedSo is null)
                            extractedSo = dest;
                    }

                    continue;
                }

                if (name.Contains("/include/", StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(".h", StringComparison.OrdinalIgnoreCase) &&
                    fileName.Length > 0)
                {
                    entry.ExtractToFile(Path.Combine(includeRoot, fileName), overwrite: true);
                }
            }
        }

        var canonical = Path.Combine(prebuiltRoot, "libraylib.so");
        if (extractedSo is not null)
        {
            if (!string.Equals(extractedSo, canonical, StringComparison.OrdinalIgnoreCase))
                File.Copy(extractedSo, canonical, overwrite: true);
        }

        if (!File.Exists(canonical))
            throw new InvalidOperationException("libraylib.so not found in linux archive (expected lib/libraylib.so*).");

        if (!File.Exists(Path.Combine(includeRoot, "raylib.h")))
            throw new InvalidOperationException("raylib.h not found in linux archive include/.");
    }

    private static async Task DownloadToFileAsync(HttpClient http, string url, string path, CancellationToken ct)
    {
        await using var s = await http.GetStreamAsync(new Uri(url), ct);
        await using var f = File.Create(path);
        await s.CopyToAsync(f, ct);
    }

    private static void ExtractZipDllAndIncludes(string zipPath, string destRoot)
    {
        using var zip = ZipFile.OpenRead(zipPath);
        var prebuiltRoot = Path.Combine(destRoot, "prebuilt", "win-x64");
        Directory.CreateDirectory(prebuiltRoot);
        var includeRoot = Path.Combine(destRoot, "include");
        Directory.CreateDirectory(includeRoot);
        foreach (var e in zip.Entries)
        {
            var n = e.FullName.Replace('\\', '/');
            if (n.EndsWith("raylib.dll", StringComparison.OrdinalIgnoreCase))
            {
                e.ExtractToFile(Path.Combine(prebuiltRoot, "raylib.dll"), overwrite: true);
                continue;
            }

            if (n.EndsWith("/include/raylib.h", StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith("/include/rlgl.h", StringComparison.OrdinalIgnoreCase) ||
                n.EndsWith("/include/raymath.h", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("/include/raylib", StringComparison.OrdinalIgnoreCase) && n.EndsWith(".h", StringComparison.OrdinalIgnoreCase))
            {
                var name = Path.GetFileName(n);
                if (name.Length > 0)
                    e.ExtractToFile(Path.Combine(includeRoot, name), overwrite: true);
            }
        }

        if (!File.Exists(Path.Combine(prebuiltRoot, "raylib.dll")))
            throw new InvalidOperationException("raylib.dll not found in zip.");
    }

    private static void FixRayguiIncludesIfNeeded(string path)
    {
        var text = File.ReadAllText(path);
        const string broken = "#include // Required for:";
        if (!text.Contains(broken, StringComparison.Ordinal))
            return;

        text = text.Replace(
            "#include // Required for: FILE, fopen(), fclose(), fprintf(), feof(), fscanf(), vsprintf() [GuiLoadStyle(), GuiLoadIcons()]",
            "#include <stdio.h> // Required for: FILE, fopen(), fclose(), fprintf(), feof(), fscanf(), vsprintf() [GuiLoadStyle(), GuiLoadIcons()]");
        text = text.Replace(
            "#include // Required for: malloc(), calloc(), free() [GuiLoadStyle(), GuiLoadIcons()]",
            "#include <stdlib.h> // Required for: malloc(), calloc(), free() [GuiLoadStyle(), GuiLoadIcons()]");
        text = text.Replace(
            "#include // Required for: strlen() [GuiTextBox(), GuiValueBox()], memset(), memcpy()",
            "#include <string.h> // Required for: strlen() [GuiTextBox(), GuiValueBox()], memset(), memcpy()");
        text = text.Replace(
            "#include // Required for: va_list, va_start(), vfprintf(), va_end() [TextFormat()]",
            "#include <stdarg.h> // Required for: va_list, va_start(), vfprintf(), va_end() [TextFormat()]");
        text = text.Replace(
            "#include // Required for: roundf() [GuiColorPicker()]",
            "#include <math.h> // Required for: roundf() [GuiColorPicker()]");
        File.WriteAllText(path, text);
    }

    private static async Task EnsureGitCloneAsync(
        PipelineContext context,
        string repoUrl,
        string destDir,
        string gitRef,
        bool recursive,
        CancellationToken cancellationToken)
    {
        if (File.Exists(Path.Combine(destDir, "src", "raycimgui.c")) || File.Exists(Path.Combine(destDir, "rlcimgui.c")))
        {
            await context.Log.WriteLineAsync($"Skipping clone; already present: {destDir}");
            return;
        }

        if (Directory.Exists(destDir))
            Directory.Delete(destDir, recursive: true);

        var cloneArgs = new List<string> { "clone", "--depth", "1", "--branch", gitRef, repoUrl, destDir };
        if (recursive)
            cloneArgs.Insert(1, "--recursive");

        var args = string.Join(' ', cloneArgs.Select(a => a.Contains(' ') ? $"\"{a}\"" : a));
        var code = await ProcessRunner.RunAsync(context, "git", args, context.RepoRoot, cancellationToken);
        if (code != 0)
            throw new InvalidOperationException($"git clone failed for {repoUrl} (exit {code})");
    }

    private sealed class VersionsManifest
    {
        public string? RaylibCimguiGitRef { get; set; }
        public string? RaylibCimguiRepoUrl { get; set; }
        public string? CimguiBundledSubdir { get; set; }
        public Dictionary<string, string>? Prebuilt { get; set; }
        public string? RayguiHeaderUrl { get; set; }
    }
}
