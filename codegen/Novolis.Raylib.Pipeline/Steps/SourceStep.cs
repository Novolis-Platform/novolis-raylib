using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class SourceStep : IPipelineStep
{
    public string Id => "step_01_source";

    public string Description => "Fetch and verify raylib 6.2 prebuilts, raygui 5.0, cimgui, ImGui, and ray-cimgui sources.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        [PipelinePaths.VersionsJson(context.RepoRoot)];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context)
    {
        var manifest = ReadManifest(context.RepoRoot);
        var cimguiSubdir = RequiredValue(manifest.CimguiBundledSubdir, "cimguiBundledSubdir");
        var list = new List<string>
        {
            PipelinePaths.RaylibHeaderPath(context.RepoRoot),
            PipelinePaths.RayguiHeaderPath(context.RepoRoot),
            Path.Combine(PipelinePaths.RaylibPrebuiltWinDir(context.RepoRoot), "raylib.dll"),
            Path.Combine(PipelinePaths.RaylibPrebuiltLinuxDir(context.RepoRoot), "libraylib.so"),
        };

        var cimgui = Path.Combine(
            PipelinePaths.RaylibCimguiRoot(context.RepoRoot),
            cimguiSubdir,
            "cimgui.h");
        list.Add(cimgui);
        return list;
    }

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var json = await ReadManifestAsync(context.RepoRoot, cancellationToken);
        ValidateArchiveLayout(json);

        var artifacts = PipelinePaths.SourceArtifactsDir(context.RepoRoot);
        ResetDirectory(artifacts);

        var raylibRoot = PipelinePaths.RaylibRoot(context.RepoRoot);
        var rayguiRoot = PipelinePaths.RayguiRoot(context.RepoRoot);
        var cimguiRoot = PipelinePaths.RaylibCimguiRoot(context.RepoRoot);
        Directory.CreateDirectory(raylibRoot);
        Directory.CreateDirectory(rayguiRoot);
        Directory.CreateDirectory(cimguiRoot);

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
            VerifySha256(zipPath, RequiredChecksum(json, "prebuilt.windows-x64"));
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
            VerifySha256(tarPath, RequiredChecksum(json, "prebuilt.linux-x64"));
            ExtractLinuxTarGz(tarPath, raylibRoot);
            File.Delete(tarPath);
            await context.Log.WriteLineAsync($"Extracted linux-x64 raylib prebuilt under {raylibRoot}");
        }
        else
            throw new InvalidOperationException("versions.json missing prebuilt.linux-x64");

        var rayguiUrl = RequiredValue(json.RayguiHeaderUrl, "rayguiHeaderUrl");
        var rayguiDest = PipelinePaths.RayguiHeaderPath(context.RepoRoot);
        await context.Log.WriteLineAsync($"Downloading {rayguiUrl}");
        await DownloadToFileAsync(http, rayguiUrl, rayguiDest, cancellationToken);
        VerifySha256(rayguiDest, RequiredChecksum(json, "raygui-header"));
        FixRayguiIncludesIfNeeded(rayguiDest);
        await context.Log.WriteLineAsync($"Wrote {rayguiDest}");

        var raylibCimguiArchiveUrl = RequiredValue(json.RaylibCimguiArchiveUrl, "raylibCimguiArchiveUrl");
        var cimguiArchiveUrl = RequiredValue(json.CimguiArchiveUrl, "cimguiArchiveUrl");
        var imguiArchiveUrl = RequiredValue(json.ImguiArchiveUrl, "imguiArchiveUrl");
        {
            var raylibCimguiArchive = Path.Combine(cimguiRoot, "raylib-cimgui.tar.gz");
            await context.Log.WriteLineAsync($"Downloading {raylibCimguiArchiveUrl}");
            await DownloadToFileAsync(http, raylibCimguiArchiveUrl, raylibCimguiArchive, cancellationToken);
            VerifySha256(raylibCimguiArchive, RequiredChecksum(json, "raylib-cimgui-archive"));
            ExtractTarGzArchive(raylibCimguiArchive, cimguiRoot);
            File.Delete(raylibCimguiArchive);

            var cimguiSubdir = RequiredValue(json.CimguiBundledSubdir, "cimguiBundledSubdir");
            var bundledCimgui = Path.Combine(cimguiRoot, cimguiSubdir);
            var cimguiArchive = Path.Combine(cimguiRoot, "cimgui.tar.gz");
            await context.Log.WriteLineAsync($"Downloading {cimguiArchiveUrl}");
            await DownloadToFileAsync(http, cimguiArchiveUrl, cimguiArchive, cancellationToken);
            VerifySha256(cimguiArchive, RequiredChecksum(json, "cimgui-archive"));
            ExtractTarGzArchive(cimguiArchive, bundledCimgui);
            File.Delete(cimguiArchive);

            var imguiSubdir = RequiredValue(json.ImguiBundledSubdir, "imguiBundledSubdir");
            var imguiRoot = Path.Combine(bundledCimgui, imguiSubdir);
            var imguiArchive = Path.Combine(cimguiRoot, "imgui.tar.gz");
            await context.Log.WriteLineAsync($"Downloading {imguiArchiveUrl}");
            await DownloadToFileAsync(http, imguiArchiveUrl, imguiArchive, cancellationToken);
            VerifySha256(imguiArchive, RequiredChecksum(json, "imgui-archive"));
            ExtractTarGzArchive(imguiArchive, imguiRoot);
            File.Delete(imguiArchive);

            var cimguiHeader = Path.Combine(bundledCimgui, "cimgui.h");
            if (!File.Exists(cimguiHeader))
                throw new InvalidOperationException($"Missing bundled cimgui at {bundledCimgui}.");
            if (!File.Exists(Path.Combine(imguiRoot, "imgui.cpp")))
                throw new InvalidOperationException($"Missing ImGui sources at {imguiRoot}.");
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

    private static VersionsManifest ReadManifest(string repoRoot)
    {
        var manifest = JsonSerializer.Deserialize<VersionsManifest>(
            File.ReadAllText(PipelinePaths.VersionsJson(repoRoot)),
            JsonSerializerOptions.Web);
        return manifest ?? throw new InvalidOperationException("versions.json could not be deserialized.");
    }

    private static async Task<VersionsManifest> ReadManifestAsync(
        string repoRoot,
        CancellationToken cancellationToken)
    {
        var manifest = JsonSerializer.Deserialize<VersionsManifest>(
            await File.ReadAllTextAsync(PipelinePaths.VersionsJson(repoRoot), cancellationToken),
            JsonSerializerOptions.Web);
        return manifest ?? throw new InvalidOperationException("versions.json could not be deserialized.");
    }

    private static string RequiredValue(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"versions.json missing {name}.")
            : value;

    private static string RequiredChecksum(VersionsManifest manifest, string name)
    {
        if (manifest.Sha256 is null ||
            !manifest.Sha256.TryGetValue(name, out var checksum) ||
            string.IsNullOrWhiteSpace(checksum))
        {
            throw new InvalidOperationException($"versions.json missing sha256.{name}.");
        }

        return checksum;
    }

    private static void ValidateArchiveLayout(VersionsManifest manifest)
    {
        var required = new[]
        {
            "prebuilt.windows-x64",
            "prebuilt.linux-x64",
            "raylib-cimgui",
            "cimgui",
            "imgui",
        };

        foreach (var key in required)
        {
            if (manifest.ArchiveLayout is null ||
                !manifest.ArchiveLayout.TryGetValue(key, out var layout) ||
                string.IsNullOrWhiteSpace(layout))
            {
                throw new InvalidOperationException($"versions.json missing archiveLayout.{key}.");
            }
        }
    }

    private static void ResetDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);

        Directory.CreateDirectory(path);
    }

    private static void VerifySha256(string path, string expected)
    {
        using var stream = File.OpenRead(path);
        var actual = Convert.ToHexString(SHA256.HashData(stream));
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"SHA-256 mismatch for {path}. Expected {expected}, actual {actual}.");
        }
    }

    private static void ExtractTarGzArchive(string tarGzPath, string destRoot)
    {
        var fullRoot = Path.GetFullPath(destRoot);
        var rootPrefix = fullRoot.EndsWith(Path.DirectorySeparatorChar)
            ? fullRoot
            : fullRoot + Path.DirectorySeparatorChar;
        Directory.CreateDirectory(fullRoot);

        using var fileStream = File.OpenRead(tarGzPath);
        using var gzip = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new System.Formats.Tar.TarReader(gzip);

        while (reader.GetNextEntry() is { } entry)
        {
            var name = entry.Name.Replace('\\', '/');
            var separator = name.IndexOf('/');
            if (separator < 0 || separator == name.Length - 1)
                continue;

            var relative = name[(separator + 1)..];
            var destination = Path.GetFullPath(
                Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!destination.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Archive entry escapes extraction root: {entry.Name}");

            if (entry.EntryType is System.Formats.Tar.TarEntryType.RegularFile or System.Formats.Tar.TarEntryType.V7RegularFile)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                entry.ExtractToFile(destination, overwrite: true);
            }
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

    private sealed class VersionsManifest
    {
        public string? CimguiArchiveUrl { get; set; }
        public string? CimguiGitCommit { get; set; }
        public string? CimguiGitRef { get; set; }
        public Dictionary<string, string>? ArchiveLayout { get; set; }
        public string? ImguiArchiveUrl { get; set; }
        public string? ImguiBundledSubdir { get; set; }
        public string? ImguiGitCommit { get; set; }
        public string? RaylibCimguiGitRef { get; set; }
        public string? RaylibCimguiGitCommit { get; set; }
        public string? RaylibCimguiArchiveUrl { get; set; }
        public string? CimguiBundledSubdir { get; set; }
        public Dictionary<string, string>? Prebuilt { get; set; }
        public string? RayguiHeaderUrl { get; set; }
        public Dictionary<string, string>? Sha256 { get; set; }
    }
}
