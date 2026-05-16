using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibInteropReflectionTests
{
    [Test]
    public async Task Raylib_manifest_import_count_matches_LibraryImport_methods()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raylib-exports.manifest.json");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var imports = doc.RootElement.GetProperty("imports");
        var names = new List<string>();
        foreach (var el in imports.EnumerateArray())
            names.Add(el.GetProperty("name").GetString()!);

        var methods = typeof(Raylib6Native)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.IsDefined(typeof(LibraryImportAttribute), inherit: false))
            .ToArray();

        await Assert.That(methods.Length).IsEqualTo(names.Count);
        foreach (var name in names)
        {
            var m = typeof(Raylib6Native).GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            await Assert.That(m).IsNotNull();
            await Assert.That(m!.IsDefined(typeof(LibraryImportAttribute), inherit: false)).IsTrue();
        }
    }

    [Test]
    public async Task Raylib_each_manifest_template_is_implemented_in_generator()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raylib-exports.manifest.json");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var imports = doc.RootElement.GetProperty("imports");
        var templates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var el in imports.EnumerateArray())
            templates.Add(el.GetProperty("template").GetString()!);

        var genPath = Path.Combine(root, "codegen", "Novolis.Raylib.CodeGen", "Emit", "RaylibInteropEmitter.cs");
        var gen = await File.ReadAllTextAsync(genPath);
        var caseLabels = new HashSet<string>(
            Regex.Matches(gen, @"case\s+""([^""]+)""\s*:")
                .Select(m => m.Groups[1].Value),
            StringComparer.Ordinal);

        foreach (var t in templates)
            await Assert.That(caseLabels.Contains(t)).IsTrue();
    }

    [Test]
    public async Task Raylib_generated_sha256_matches_manifest_file_bytes()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raylib-exports.manifest.json");
        var manifestBytes = await File.ReadAllBytesAsync(manifestPath);
        var expected = Convert.ToHexString(SHA256.HashData(manifestBytes)).ToLowerInvariant();

        var genPath = Path.Combine(
            root,
            "src", "Novolis.Raylib.Bindings", "Interop",
            "Raylib6Native.g.cs");
        var gen = await File.ReadAllTextAsync(genPath);
        var line = gen.Split('\n').FirstOrDefault(l => l.Contains("// ManifestSha256:", StringComparison.Ordinal));
        await Assert.That(line).IsNotNull();
        await Assert.That(line!.Contains(expected, StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task Raygui_function_count_matches_export_pointer_fields()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raygui-exports.manifest.json");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var functions = doc.RootElement.GetProperty("functions");
        var exports = new List<string>();
        foreach (var el in functions.EnumerateArray())
            exports.Add(el.GetProperty("export").GetString()!);

        var ptrFields = typeof(RayguiShimExports)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.Name.EndsWith("_ptr", StringComparison.Ordinal))
            .ToArray();

        await Assert.That(ptrFields.Length).IsEqualTo(exports.Count);
        foreach (var ex in exports)
            await Assert.That(ptrFields.Any(f => f.Name == $"{ex}_ptr")).IsTrue();
    }

    [Test]
    public async Task Raygui_each_manifest_template_is_implemented_in_generator()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var manifestPath = Path.Combine(root, "pipeline", "raylib6", "raygui-exports.manifest.json");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var functions = doc.RootElement.GetProperty("functions");
        var templates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var el in functions.EnumerateArray())
            templates.Add(el.GetProperty("template").GetString()!);

        var genPath = Path.Combine(root, "codegen", "Novolis.Raylib.CodeGen", "Emit", "RayguiInteropEmitter.cs");
        var gen = await File.ReadAllTextAsync(genPath);
        var blockStart = gen.IndexOf(
            "static (string delegateType, string exportName) TemplateToDelegate",
            StringComparison.Ordinal);
        await Assert.That(blockStart).IsGreaterThanOrEqualTo(0);
        var throwIdx = gen.IndexOf("_ => throw new InvalidOperationException", blockStart, StringComparison.Ordinal);
        await Assert.That(throwIdx).IsGreaterThanOrEqualTo(0);
        var block = gen.Substring(blockStart, throwIdx - blockStart + 80);
        var caseLabels = new HashSet<string>(
            Regex.Matches(block, @"""([a-z0-9_]+)""\s*=>")
                .Select(m => m.Groups[1].Value),
            StringComparer.Ordinal);

        foreach (var t in templates)
            await Assert.That(caseLabels.Contains(t)).IsTrue();
    }
}
