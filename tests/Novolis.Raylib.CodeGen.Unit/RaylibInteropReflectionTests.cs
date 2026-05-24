using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibInteropReflectionTests
{
    [Test]
    public async Task Raylib_manifest_import_count_matches_LibraryImport_methods()
    {
        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");
        var names = interop.Imports.Select(i => i.Name).ToList();

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

        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");
        var templates = interop.Imports.Select(i => i.Template).ToHashSet(StringComparer.Ordinal);

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
    public async Task Raylib_generated_sha256_matches_manifest_fingerprint()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");
        var expected = interop.Sha256Hex();

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
        var raygui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "raygui");
        var exports = raygui.Exports.Select(e => e.Export).ToList();

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

        var raygui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "raygui");
        var templates = raygui.Exports.Select(e => e.Template).ToHashSet(StringComparer.Ordinal);

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

    [Test]
    public async Task Imgui_function_count_matches_export_pointer_fields()
    {
        var imgui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "imgui");
        var exports = imgui.Exports.Select(e => e.Export).ToList();

        var ptrFields = typeof(ImguiShimExports)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(f => f.Name.EndsWith("_ptr", StringComparison.Ordinal))
            .ToArray();

        await Assert.That(ptrFields.Length).IsEqualTo(exports.Count);
        foreach (var ex in exports)
            await Assert.That(ptrFields.Any(f => f.Name == $"{ex}_ptr")).IsTrue();
    }

    [Test]
    public async Task Imgui_each_manifest_template_is_implemented_in_generator()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var imgui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "imgui");
        var templates = imgui.Exports.Select(e => e.Template).ToHashSet(StringComparer.Ordinal);

        var genPath = Path.Combine(root, "codegen", "Novolis.Raylib.CodeGen", "Emit", "ImguiInteropEmitter.cs");
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
