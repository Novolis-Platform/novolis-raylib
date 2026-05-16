// Fail if raylib-exports.manifest.json lists symbols missing from vendor raylib.h (when header exists).
// Usage (repo root): dotnet run tools/raylib6-pipeline/verify-raylib-manifest-against-vendor.cs
#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property PublishAot=false
#:property PackAsTool=false
#:property ImplicitUsings=enable
#:property Nullable=enable
#:property ManagePackageVersionsCentrally=false

using System.Text.Json;
using System.Text.Json.Serialization;

if (args is ["-h"] or ["--help"])
{
	Console.WriteLine("""
		verify-raylib-manifest-against-vendor — ensures each manifest import name appears in tools/vendor/raylib-6/include/raylib.h
		Skips with exit 0 if the header file is missing (e.g. before fetch).
		""");
	return 0;
}

var repoRoot = FindRepoRoot();
var manifestPath = Path.Combine(repoRoot, "tools", "raylib6-pipeline", "raylib-exports.manifest.json");
var headerPath = Path.Combine(repoRoot, "tools", "vendor", "raylib-6", "include", "raylib.h");
if (!File.Exists(manifestPath))
{
	Console.Error.WriteLine($"Missing manifest: {manifestPath}");
	return 2;
}

if (!File.Exists(headerPath))
{
	Console.WriteLine($"verify-raylib-manifest: skip (no vendor header at {headerPath}).");
	return 0;
}

var json = File.ReadAllText(manifestPath);
var doc = JsonSerializer.Deserialize<ManifestRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
if (doc?.Imports is null || doc.Imports.Count == 0)
{
	Console.Error.WriteLine("Manifest has no imports.");
	return 3;
}

var header = File.ReadAllText(headerPath);
foreach (var imp in doc.Imports.OrderBy(i => i.Name, StringComparer.Ordinal))
{
	if (string.IsNullOrEmpty(imp.Name))
		continue;

	if (!HeaderDeclaresSymbol(header, imp.Name))
	{
		Console.Error.WriteLine($"verify-raylib-manifest: '{imp.Name}' not found as RLAPI declaration in raylib.h.");
		return 4;
	}
}

Console.WriteLine($"verify-raylib-manifest: OK ({doc.Imports.Count} imports).");
return 0;

static bool HeaderDeclaresSymbol(string header, string symbol)
{
	var needle = symbol + "(";
	foreach (var raw in header.Split('\n'))
	{
		var line = raw.TrimStart();
		if (line.StartsWith("RLAPI", StringComparison.Ordinal) && line.Contains(needle, StringComparison.Ordinal))
			return true;
	}

	return false;
}

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

internal sealed class ManifestRoot
{
	[JsonPropertyName("imports")]
	public List<ImportEntry>? Imports { get; set; }
}

internal sealed class ImportEntry
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }
}
