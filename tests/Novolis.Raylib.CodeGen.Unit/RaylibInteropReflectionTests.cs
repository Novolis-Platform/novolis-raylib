using System.Reflection;
using System.Runtime.InteropServices;
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
        var names = interop.Imports.Select(import => import.Name).ToList();

        var methods = typeof(Raylib6Native)
            .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(method => method.IsDefined(typeof(LibraryImportAttribute), inherit: false))
            .ToArray();

        await Assert.That(methods.Length).IsEqualTo(names.Count);
        foreach (var name in names)
        {
            var method = typeof(Raylib6Native).GetMethod(
                name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            await Assert.That(method).IsNotNull();
            await Assert.That(method!.IsDefined(typeof(LibraryImportAttribute), inherit: false)).IsTrue();
        }
    }

    [Test]
    public async Task Raylib_manifest_uses_complete_typed_c_abi_signatures()
    {
        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");

        await Assert.That(interop.Imports).IsNotEmpty();
        foreach (var import in interop.Imports)
        {
            await Assert.That(import.Signature.ReturnType).IsNotNull();
            await Assert.That(import.Signature.Parameters.All(parameter => !string.IsNullOrWhiteSpace(parameter.Name))).IsTrue();
        }

        var beginMode = interop.Imports.Single(import => import.Name == "BeginMode3D");
        await Assert.That(beginMode.Signature.Parameters[0].Name).IsEqualTo("camera");
        await Assert.That(beginMode.Signature.Parameters[0].Type.CSharpTypeName).IsEqualTo("Camera");
    }

    [Test]
    public async Task Raylib_generated_sha256_matches_manifest_fingerprint()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");
        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");

        var generatedPath = Path.Combine(
            root,
            "src",
            "Novolis.Raylib.Bindings",
            "Interop",
            "Raylib6Native.g.cs");
        var generated = await File.ReadAllTextAsync(generatedPath);
        var line = generated.Split('\n').FirstOrDefault(value => value.Contains("// ManifestSha256:", StringComparison.Ordinal));

        await Assert.That(line).IsNotNull();
        await Assert.That(line!).Contains(interop.Sha256Hex());
    }

    [Test]
    public async Task Raygui_function_count_matches_export_pointer_fields()
    {
        var raygui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "raygui");
        var fields = typeof(RayguiShimExports)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.Name.EndsWith("_ptr", StringComparison.Ordinal))
            .ToArray();

        await Assert.That(fields.Length).IsEqualTo(raygui.Exports.Count);
        await Assert.That(raygui.EmbeddedTypes).IsNotNull();
        await Assert.That(raygui.EmbeddedTypes!.Count).IsEqualTo(1);
        await Assert.That(raygui.EmbeddedTypes![0].Name).IsEqualTo("RayguiRectangle");
        foreach (var export in raygui.Exports)
            await Assert.That(fields.Any(field => field.Name == $"{export.Export}_ptr")).IsTrue();
    }

    [Test]
    public async Task Imgui_function_count_matches_export_pointer_fields()
    {
        var imgui = RaylibBindingManifestSource.Instance.GetRequired<ShimExportsFragment>(
            FragmentKind.ShimExports,
            "imgui");
        var fields = typeof(ImguiShimExports)
            .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.Name.EndsWith("_ptr", StringComparison.Ordinal))
            .ToArray();

        await Assert.That(fields.Length).IsEqualTo(imgui.Exports.Count);
        await Assert.That(imgui.Exports.All(export => export.Signature.Parameters is not null)).IsTrue();
        foreach (var export in imgui.Exports)
            await Assert.That(fields.Any(field => field.Name == $"{export.Export}_ptr")).IsTrue();
    }
}
