// Fetches pinned raylib 6 prebuilts + raygui header into tools/vendor (see versions.json).
// Usage (repo root): dotnet run tools/raylib6-pipeline/fetch-sources.cs
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

var repoRoot = FindRepoRoot();
var manifestPath = Path.Combine(repoRoot, "pipeline", "raylib6", "versions.json");
if (args is ["-h"] or ["--help"])
{
	Console.WriteLine("Usage: dotnet run tools/raylib6-pipeline/fetch-sources.cs");
	return 0;
}

if (!File.Exists(manifestPath))
{
	Console.Error.WriteLine($"Missing {manifestPath}");
	return 2;
}

var json = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(manifestPath), JsonSerializerOptions.Web)!;
var vendorRaylib = Path.Combine(repoRoot, "vendor", "raylib-6");
var vendorRaygui = Path.Combine(repoRoot, "vendor", "raygui-6");
var vendorCimgui = Path.Combine(repoRoot, "vendor", "cimgui");
var vendorRaylibCimgui = Path.Combine(repoRoot, "vendor", "raylib-cimgui");
Directory.CreateDirectory(vendorRaylib);
Directory.CreateDirectory(vendorRaygui);

using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

if (OperatingSystem.IsWindows())
{
	if (json.Prebuilt is null || !json.Prebuilt.TryGetValue("windows-x64", out var url) || string.IsNullOrEmpty(url))
	{
		Console.Error.WriteLine("versions.json missing prebuilt.windows-x64");
		return 3;
	}

	var zipPath = Path.Combine(vendorRaylib, "download_win.zip");
	Console.WriteLine($"Downloading {url}");
	await DownloadToFileAsync(http, url, zipPath);
	ExtractZipDllAndIncludes(zipPath, vendorRaylib);
	File.Delete(zipPath);
	Console.WriteLine($"Extracted raylib 6 prebuilt under {vendorRaylib}");
}

if (json.RayguiHeaderUrl is { } rayguiUrl)
{
	var dest = Path.Combine(vendorRaygui, "raygui.h");
	Console.WriteLine($"Downloading {rayguiUrl}");
	await DownloadToFileAsync(http, rayguiUrl, dest);
	FixRayguiIncludesIfNeeded(dest);
	Console.WriteLine($"Wrote {dest}");
}

if (json.CimguiRepoUrl is { } cimguiRepo)
{
	EnsureGitClone(cimguiRepo, vendorCimgui, json.CimguiGitRef ?? "master", recursive: true);
	Console.WriteLine($"cimgui ready at {vendorCimgui}");
}

if (json.RaylibCimguiRepoUrl is { } raylibCimguiRepo)
{
	EnsureGitClone(raylibCimguiRepo, vendorRaylibCimgui, json.RaylibCimguiGitRef ?? "master", recursive: false);
	Console.WriteLine($"raylib-cimgui ready at {vendorRaylibCimgui}");
}

Console.WriteLine("fetch-sources: done.");
return 0;

static string FindRepoRoot()
{
	var dir = new DirectoryInfo(AppContext.BaseDirectory);
	while (dir is not null)
	{
		if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
			return dir.FullName;
		dir = dir.Parent;
	}

	return Directory.GetCurrentDirectory();
}

static async Task DownloadToFileAsync(HttpClient http, string url, string path)
{
	await using var s = await http.GetStreamAsync(new Uri(url));
	await using var f = File.Create(path);
	await s.CopyToAsync(f);
}

static void ExtractZipDllAndIncludes(string zipPath, string destRoot)
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
		throw new InvalidOperationException("raylib.dll not found in zip — update versions.json prebuilt URL or extraction logic.");
}

/// <summary>Upstream raw fetch can strip angle brackets in RAYGUI_IMPLEMENTATION includes; repair if detected.</summary>
static void FixRayguiIncludesIfNeeded(string path)
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

static void EnsureGitClone(string repoUrl, string destDir, string gitRef, bool recursive)
{
	if (File.Exists(Path.Combine(destDir, "cimgui.h")) || File.Exists(Path.Combine(destDir, "rlcimgui.c")))
	{
		Console.WriteLine($"Skipping clone; already present: {destDir}");
		return;
	}

	if (Directory.Exists(destDir))
		Directory.Delete(destDir, recursive: true);

	var cloneArgs = new List<string> { "clone", "--depth", "1", "--branch", gitRef, repoUrl, destDir };
	if (recursive)
		cloneArgs.Insert(1, "--recursive");

	var code = RunProcess("git", string.Join(' ', cloneArgs.Select(a => a.Contains(' ') ? $"\"{a}\"" : a)), FindRepoRoot());
	if (code != 0)
		throw new InvalidOperationException($"git clone failed for {repoUrl} (exit {code})");
}

static int RunProcess(string file, string arguments, string workingDirectory)
{
	var psi = new System.Diagnostics.ProcessStartInfo
	{
		FileName = file,
		Arguments = arguments,
		WorkingDirectory = workingDirectory,
		UseShellExecute = false,
	};
	using var p = System.Diagnostics.Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {file}");
	p.WaitForExit();
	return p.ExitCode;
}

sealed class Manifest
{
	public string? RaylibGitTag { get; set; }
	public string? RayguiGitTag { get; set; }
	public string? CimguiGitRef { get; set; }
	public string? RaylibCimguiGitRef { get; set; }
	public string? CimguiRepoUrl { get; set; }
	public string? RaylibCimguiRepoUrl { get; set; }
	public Dictionary<string, string>? Prebuilt { get; set; }
	public string? RayguiHeaderUrl { get; set; }
}
