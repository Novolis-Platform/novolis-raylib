using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Raylib.CodeGen;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibBindingParityTests
{
    private static readonly string[] GeneratedRelativePaths =
    [
        "src/Novolis.Raylib.Bindings/Interop/Raylib6Native.g.cs",
        "src/Novolis.Raylib.Bindings/Interop/ImguiShimExports.g.cs",
        "src/Novolis.Raylib.Bindings/Interop/RaylibDebugFrameHooks.g.cs",
        "src/Novolis.Raylib.Runtime/Rendering/Graphics.g.cs",
        "src/Novolis.Raylib.Runtime/Rendering/Textures.g.cs",
        "src/Novolis.Raylib.Runtime/Rendering/World.g.cs",
        "src/Novolis.Raylib.Runtime/Windowing/Window.g.cs",
        "src/Novolis.Raylib.Runtime/Interact/Input.g.cs",
        "src/Novolis.Raylib.Runtime/Timing/Time.g.cs",
        "src/Novolis.Raylib.Runtime/Audio/AudioDevice.g.cs",
        "src/Novolis.Raylib.Runtime/Hud/Hud.g.cs",
        "src/Novolis.Raylib.Runtime/Gui/Gui.g.cs",
        "src/Novolis.Raylib.Raygui/Interop/RayguiShimExports.g.cs",
        "src/Novolis.Raylib.Raygui/RayGui/RayGui.g.cs",
    ];

    [Test]
    public async Task T1_all_fourteen_outputs_are_structurally_equivalent_after_host_emit()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var committed = SnapshotGenerated(root);
        try
        {
            var host = new RaylibBindingCodegenHost();
            host.GenerateBindingsOnly(new BindingCodegenOptions
            {
                RepoRoot = root,
                IncludeRaygui = true,
                VerifyManifest = false,
                RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate",
            });

            foreach (var relativePath in GeneratedRelativePaths)
            {
                var fullPath = Path.Combine(root, relativePath);
                await Assert.That(File.Exists(fullPath)).IsTrue();
                var before = committed[relativePath];
                var after = await File.ReadAllTextAsync(fullPath);
                var equivalent = CompilationUnitComparer.AreStructurallyEquivalent(before, after);
                await Assert.That(equivalent).IsTrue().Because($"T1 parity failed for {relativePath}");
            }
        }
        finally
        {
            RestoreGenerated(root, committed);
        }
    }

    [Test]
    public async Task T1_without_raygui_skips_optional_outputs()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");

        var rayguiManifest = Path.Combine(PipelinePaths.PipelineRaylibDir(root), "raygui-exports.manifest.json");
        if (!File.Exists(rayguiManifest))
        {
            await Assert.That(true).IsTrue();
            return;
        }

        var committed = SnapshotGenerated(root);
        try
        {
            var host = new RaylibBindingCodegenHost();
            host.GenerateBindingsOnly(new BindingCodegenOptions
            {
                RepoRoot = root,
                IncludeRaygui = false,
                VerifyManifest = false,
                RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.Pipeline -- run generate",
            });

            foreach (var relativePath in GeneratedRelativePaths.Where(p => !p.Contains("Raygui", StringComparison.Ordinal)))
            {
                var fullPath = Path.Combine(root, relativePath);
                var before = committed[relativePath];
                var after = await File.ReadAllTextAsync(fullPath);
                await Assert.That(CompilationUnitComparer.AreStructurallyEquivalent(before, after)).IsTrue()
                    .Because($"T1 parity failed for {relativePath} (IncludeRaygui=false)");
            }

            await Assert.That(File.Exists(Path.Combine(root, "src/Novolis.Raylib.Raygui/Interop/RayguiShimExports.g.cs")))
                .IsTrue()
                .Because("optional raygui outputs should remain untouched when IncludeRaygui=false");
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(root, "src/Novolis.Raylib.Raygui/Interop/RayguiShimExports.g.cs")))
                .IsEqualTo(committed["src/Novolis.Raylib.Raygui/Interop/RayguiShimExports.g.cs"]);
        }
        finally
        {
            RestoreGenerated(root, committed);
        }
    }

    private static Dictionary<string, string> SnapshotGenerated(string root)
    {
        var snapshot = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var relativePath in GeneratedRelativePaths)
            snapshot[relativePath] = File.ReadAllText(Path.Combine(root, relativePath));
        return snapshot;
    }

    private static void RestoreGenerated(string root, IReadOnlyDictionary<string, string> snapshot)
    {
        foreach (var (relativePath, content) in snapshot)
            File.WriteAllText(Path.Combine(root, relativePath), content);
    }
}
