using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibCodegenPipeline
{
    private readonly string _repoRoot;
    private readonly IReadOnlyList<IRaylibCodegenHook> _hooks;

    public RaylibCodegenPipeline(string repoRoot, IReadOnlyList<IRaylibCodegenHook>? hooks = null)
    {
        _repoRoot = repoRoot;
        _hooks = hooks ?? HookDiscovery.DiscoverAll();
    }

    public int GenerateAll()
    {
        var verify = RaylibManifestVerifier.Verify(_repoRoot);
        if (verify != 0)
            return verify;

        EmitRaylibInterop();
        EmitImguiInterop();
        EmitRayguiInterop();
        EmitDebugHooks();
        EmitFacades();
        EmitHud();
        EmitGui();
        EmitRayGui();
        return 0;
    }

    public void EmitRaylibInterop()
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "raylib-exports.manifest.json");
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var outPath = Path.Combine(RepoPaths.InteropDir(_repoRoot), "Raylib6Native.g.cs");
        var descriptions = RaylibManifestModels.LoadImportDescriptions(manifestPath);

        var source = RaylibInteropEmitter.Emit(manifestPath, manifestBytes, manifestSha256);
        var context = new RaylibCodegenContext
        {
            RepoRoot = _repoRoot,
            Phase = RaylibCodegenPhase.Interop,
            OutputPath = outPath,
            ManifestPath = manifestPath,
            ManifestSha256 = manifestSha256,
            RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
            ImportDescriptions = descriptions,
        };

        WriteUnit(source, context);
        Console.WriteLine($"Wrote {RaylibManifestModels.LoadImports(manifestPath).Count} imports to {outPath}");
    }

    public void EmitImguiInterop()
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "imgui-exports.manifest.json");
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var outPath = Path.Combine(RepoPaths.InteropDir(_repoRoot), "ImguiShimExports.g.cs");

        var source = ImguiInteropEmitter.Emit(manifestPath, manifestBytes, manifestSha256);
        var context = new RaylibCodegenContext
        {
            RepoRoot = _repoRoot,
            Phase = RaylibCodegenPhase.ImGui,
            OutputPath = outPath,
            ManifestPath = manifestPath,
            ManifestSha256 = manifestSha256,
            RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
        };

        WriteUnit(source, context);
        Console.WriteLine($"Wrote Imgui interop to {outPath}");
    }

    public void EmitRayguiInterop()
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "raygui-exports.manifest.json");
        if (!File.Exists(manifestPath))
        {
            Console.WriteLine("raygui-exports.manifest.json not found; skipping raygui interop.");
            return;
        }

        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var rayguiDir = Path.Combine(_repoRoot, "src", "Novolis.Raylib.Raygui", "Interop");
        Directory.CreateDirectory(rayguiDir);
        var outPath = Path.Combine(rayguiDir, "RayguiShimExports.g.cs");

        var source = RayguiInteropEmitter.Emit(manifestPath, manifestBytes, manifestSha256);
        var context = new RaylibCodegenContext
        {
            RepoRoot = _repoRoot,
            Phase = RaylibCodegenPhase.Raygui,
            OutputPath = outPath,
            ManifestPath = manifestPath,
            ManifestSha256 = manifestSha256,
            RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
        };

        WriteUnit(source, context);
        var count = RayguiManifestModels.LoadFunctions(manifestPath).Count;
        Console.WriteLine($"Wrote {count} exports to {outPath}");
    }

    public void EmitDebugHooks()
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "raylib-debug.manifest.json");
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var outPath = Path.Combine(RepoPaths.InteropDir(_repoRoot), "RaylibDebugFrameHooks.g.cs");

        var source = RaylibDebugHooksEmitter.Emit(manifestPath, manifestBytes, manifestSha256);
        var context = new RaylibCodegenContext
        {
            RepoRoot = _repoRoot,
            Phase = RaylibCodegenPhase.Debug,
            OutputPath = outPath,
            ManifestPath = manifestPath,
            ManifestSha256 = manifestSha256,
            RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
        };

        WriteUnit(source, context);
        Console.WriteLine($"Wrote {outPath}");
    }

    private (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) LoadHeaderDocs() =>
        (
            RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RaylibHeaderPath(_repoRoot)),
            RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RayguiHeaderPath(_repoRoot)));

    public void EmitFacades()
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "facades.manifest.json");
        if (!File.Exists(manifestPath))
        {
            Console.WriteLine("facades.manifest.json not found; skipping façade emit.");
            return;
        }

        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var types = FacadeManifestModels.LoadTypes(manifestPath);
        var (raylibComments, rayguiComments) = LoadHeaderDocs();
        var raylibManifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "raylib-exports.manifest.json");
        var facadeMethodImpl = File.Exists(raylibManifestPath)
            ? RaylibManifestModels.LoadInteropPolicy(raylibManifestPath).FacadeMethodImpl
            : null;

        foreach (var facadeType in types)
        {
            var outPath = Path.Combine(RepoPaths.RuntimeDir(_repoRoot), facadeType.Folder, $"{facadeType.Name}.g.cs");
            var source = FacadeEmitter.EmitType(facadeType, manifestSha256, raylibComments, rayguiComments, facadeMethodImpl);
            var context = new RaylibCodegenContext
            {
                RepoRoot = _repoRoot,
                Phase = RaylibCodegenPhase.Facade,
                OutputPath = outPath,
                ManifestPath = manifestPath,
                ManifestSha256 = manifestSha256,
                RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
                FacadeTypeName = facadeType.Name,
                FacadeMethodImpl = facadeMethodImpl,
            };

            WriteUnit(source, context);
            Console.WriteLine($"Wrote façade {facadeType.Name} to {outPath}");
        }
    }

    public void EmitHud()
    {
        EmitManifestTypes("hud.manifest.json", "Hud");
    }

    public void EmitGui()
    {
        EmitManifestTypes("gui.manifest.json", "Gui");
    }

    public void EmitRayGui()
    {
        EmitManifestTypes(
            "raygui.manifest.json",
            "RayGui",
            Path.Combine(_repoRoot, "src", "Novolis.Raylib.Raygui"));
    }

    private void EmitManifestTypes(string manifestFileName, string label, string? outputRoot = null)
    {
        var manifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), manifestFileName);
        if (!File.Exists(manifestPath))
        {
            Console.WriteLine($"{manifestFileName} not found; skipping {label} emit.");
            return;
        }

        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = Sha256Hex(manifestBytes);
        var types = FacadeManifestModels.LoadTypes(manifestPath);
        var (raylibComments, rayguiComments) = LoadHeaderDocs();
        var raylibManifestPath = Path.Combine(RepoPaths.PipelineDir(_repoRoot), "raylib-exports.manifest.json");
        var facadeMethodImpl = File.Exists(raylibManifestPath)
            ? RaylibManifestModels.LoadInteropPolicy(raylibManifestPath).FacadeMethodImpl
            : null;

        var root = outputRoot ?? RepoPaths.RuntimeDir(_repoRoot);
        foreach (var facadeType in types)
        {
            var outPath = Path.Combine(root, facadeType.Folder, $"{facadeType.Name}.g.cs");
            var source = FacadeEmitter.EmitType(facadeType, manifestSha256, raylibComments, rayguiComments, facadeMethodImpl);
            var context = new RaylibCodegenContext
            {
                RepoRoot = _repoRoot,
                Phase = RaylibCodegenPhase.Facade,
                OutputPath = outPath,
                ManifestPath = manifestPath,
                ManifestSha256 = manifestSha256,
                RegenerateHint = "dotnet run --project codegen/Novolis.Raylib.CodeGen -- generate",
                FacadeTypeName = facadeType.Name,
                FacadeMethodImpl = facadeMethodImpl,
            };

            WriteUnit(source, context);
            Console.WriteLine($"Wrote {label} façade {facadeType.Name} to {outPath}");
        }
    }

    private void WriteUnit(string source, RaylibCodegenContext context)
    {
        var unit = CodegenFormatter.ParseGenerated(source);
        foreach (var hook in _hooks.Where(h => h.Phase == context.Phase))
            unit = hook.Transform(unit, context);

        var formatted = context.Phase == RaylibCodegenPhase.Facade
            ? unit.NormalizeWhitespace(eol: Environment.NewLine).ToFullString()
            : CodegenFormatter.FormatCompilationUnit(unit);
        Directory.CreateDirectory(Path.GetDirectoryName(context.OutputPath)!);
        if (!formatted.EndsWith('\n'))
            formatted += Environment.NewLine;
        File.WriteAllText(context.OutputPath, formatted, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string Sha256Hex(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}
