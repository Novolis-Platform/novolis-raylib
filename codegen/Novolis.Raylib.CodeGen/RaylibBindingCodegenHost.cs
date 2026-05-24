using System.Reflection;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.CodeGen.Pipeline;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.LibraryImport;

    public string Emit(EmitRequest request) =>
        RaylibInteropEmitter.Emit(request.ManifestPath, request.ManifestBytes, request.ManifestSha256);
}

internal sealed class ImguiInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DynamicExports;

    public string Emit(EmitRequest request) =>
        ImguiInteropEmitter.Emit(request.ManifestPath, request.ManifestBytes, request.ManifestSha256);
}

internal sealed class RayguiInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DynamicExports;

    public string Emit(EmitRequest request) =>
        RayguiInteropEmitter.Emit(request.ManifestPath, request.ManifestBytes, request.ManifestSha256);
}

internal sealed class RaylibDebugHooksEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DebugHooks;

    public string Emit(EmitRequest request) =>
        RaylibDebugHooksEmitter.Emit(request.ManifestPath, request.ManifestBytes, request.ManifestSha256);
}

internal sealed class FacadeEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.FacadeForward;

    private readonly FacadeTypeDefinition _type;
    private readonly IReadOnlyDictionary<string, string> _raylibComments;
    private readonly IReadOnlyDictionary<string, string> _rayguiComments;
    private readonly string? _facadeMethodImpl;

    public FacadeEmitterAdapter(
        FacadeTypeDefinition type,
        IReadOnlyDictionary<string, string> raylibComments,
        IReadOnlyDictionary<string, string> rayguiComments,
        string? facadeMethodImpl)
    {
        _type = type;
        _raylibComments = raylibComments;
        _rayguiComments = rayguiComments;
        _facadeMethodImpl = facadeMethodImpl;
    }

    public string Emit(EmitRequest request) =>
        FacadeEmitter.EmitType(_type, request.ManifestSha256, _raylibComments, _rayguiComments, _facadeMethodImpl);
}

public sealed class RaylibBindingCodegenHost : IBindingCodegenHost
{
    private readonly IReadOnlyList<IRaylibCodegenHook> _hooks;

    public RaylibBindingCodegenHost(IReadOnlyList<IRaylibCodegenHook>? hooks = null)
    {
        _hooks = hooks ?? RaylibHookDiscovery.DiscoverAll();
    }

    public int GenerateAll(BindingCodegenOptions options, TextWriter? log = null)
    {
        if (options.VerifyManifest)
        {
            var verify = RaylibManifestVerifier.Verify(options.RepoRoot);
            if (verify != 0)
                return verify;
        }

        GenerateBindingsOnly(options, log);
        return 0;
    }

    public void GenerateBindingsOnly(BindingCodegenOptions options, TextWriter? log = null)
    {
        BindingCodegenExecutor.ValidateCompanions(BuildProject(), options.RepoRoot);

        var debugConfig = LoadDebugConfig(options.RepoRoot);
        var facadeMethodImpl = LoadFacadeMethodImpl(options.RepoRoot);
        var headerDocs = LoadHeaderDocs(options.RepoRoot);

        foreach (var job in AllJobs().Where(j => !j.Optional || options.IncludeRaygui))
        {
            var manifestPath = Path.Combine(RepoPaths.PipelineDir(options.RepoRoot), job.ManifestFileName);
            if (job.Optional && !File.Exists(manifestPath))
            {
                log?.WriteLine($"skip: {job.Label} (optional manifest missing)");
                continue;
            }

            log?.WriteLine($"emit: {job.Label}");
            job.Emit(options, manifestPath, debugConfig, facadeMethodImpl, headerDocs, _hooks);
        }
    }

    internal static BindingProject BuildProject() =>
        BindingProject.Create("Novolis.Raylib")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/Utf8StringMarshaller.cs", "UTF-8 marshalling")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibColor.cs", "Raylib color type")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibInteropMarshaling.cs", "Interop helpers")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibDebugCaptureGate.cs", "Debug capture gate")
            .RequireCompanion("src/Novolis.Raylib.Runtime/ImguiShimHost.cs", "ImGui native host")
            .RequireCompanion("src/Novolis.Raylib.Runtime/Gui/GuiControls.cs", "ImGui controls layer")
            .RequireCompanion("src/Novolis.Raylib.Raygui/RayGuiControls.cs", "Raygui controls layer");

    private static DebugConfigFragment? LoadDebugConfig(string repoRoot)
    {
        var path = Path.Combine(RepoPaths.PipelineDir(repoRoot), "raylib-debug.manifest.json");
        return File.Exists(path) ? DebugConfigSerializer.LoadFromFile(path) : null;
    }

    private static string? LoadFacadeMethodImpl(string repoRoot)
    {
        var path = Path.Combine(RepoPaths.PipelineDir(repoRoot), "raylib-exports.manifest.json");
        return File.Exists(path)
            ? RaylibManifestModels.LoadInteropPolicy(path).FacadeMethodImpl
            : null;
    }

    private static (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) LoadHeaderDocs(
        string repoRoot) =>
        (
            RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RaylibHeaderPath(repoRoot)),
            RaylibHeaderDocs.LoadFromFile(RaylibHeaderDocs.RayguiHeaderPath(repoRoot)));

    private sealed record EmitJob(
        string Label,
        string ManifestFileName,
        Action<BindingCodegenOptions, string, DebugConfigFragment?, string?, (IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, string>), IReadOnlyList<IRaylibCodegenHook>> Emit,
        bool Optional = false);

    private static IEnumerable<EmitJob> AllJobs()
    {
        yield return new EmitJob(
            "raylib interop",
            "raylib-exports.manifest.json",
            static (options, manifestPath, debugConfig, _, _, hooks) =>
                EmitInterop(options, manifestPath, debugConfig, hooks));

        yield return new EmitJob(
            "imgui interop",
            "imgui-exports.manifest.json",
            static (options, manifestPath, debugConfig, _, _, hooks) =>
                EmitSimple(options, manifestPath, RaylibCodegenPhase.ImGui,
                    new ImguiInteropEmitterAdapter(), debugConfig, hooks));

        yield return new EmitJob(
            "raygui interop",
            "raygui-exports.manifest.json",
            static (options, manifestPath, debugConfig, _, _, hooks) =>
                EmitRayguiInterop(options, manifestPath, debugConfig, hooks),
            Optional: true);

        yield return new EmitJob(
            "debug hooks",
            "raylib-debug.manifest.json",
            static (options, manifestPath, debugConfig, _, _, hooks) =>
                EmitSimple(options, manifestPath, RaylibCodegenPhase.Debug,
                    new RaylibDebugHooksEmitterAdapter(), debugConfig, hooks));

        yield return new EmitJob(
            "facades",
            "facades.manifest.json",
            static (options, manifestPath, debugConfig, facadeMethodImpl, headerDocs, hooks) =>
                EmitManifestTypesInternal(options, manifestPath, RepoPaths.RuntimeDir(options.RepoRoot), debugConfig, facadeMethodImpl, headerDocs, hooks, "facade"));

        yield return new EmitJob(
            "hud",
            "hud.manifest.json",
            static (options, manifestPath, debugConfig, facadeMethodImpl, headerDocs, hooks) =>
                EmitManifestTypesInternal(options, manifestPath, RepoPaths.RuntimeDir(options.RepoRoot), debugConfig, facadeMethodImpl, headerDocs, hooks, "Hud"));

        yield return new EmitJob(
            "gui",
            "gui.manifest.json",
            static (options, manifestPath, debugConfig, facadeMethodImpl, headerDocs, hooks) =>
                EmitManifestTypesInternal(options, manifestPath, RepoPaths.RuntimeDir(options.RepoRoot), debugConfig, facadeMethodImpl, headerDocs, hooks, "Gui"));

        yield return new EmitJob(
            "raygui",
            "raygui.manifest.json",
            static (options, manifestPath, debugConfig, facadeMethodImpl, headerDocs, hooks) =>
                EmitManifestTypesInternal(
                    options,
                    manifestPath,
                    Path.Combine(options.RepoRoot, "src", "Novolis.Raylib.Raygui"),
                    debugConfig,
                    facadeMethodImpl,
                    headerDocs,
                    hooks,
                    "RayGui"),
            Optional: true);
    }

    private static void EmitInterop(
        BindingCodegenOptions options,
        string manifestPath,
        DebugConfigFragment? debugConfig,
        IReadOnlyList<IRaylibCodegenHook> hooks)
    {
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = StepFileFingerprint.Sha256Hex(manifestBytes);
        var outPath = Path.Combine(RepoPaths.InteropDir(options.RepoRoot), "Raylib6Native.g.cs");
        var descriptions = RaylibManifestModels.LoadImportDescriptions(manifestPath);
        var context = CreateContext(options, RaylibCodegenPhase.Interop, outPath, manifestPath, manifestSha256, debugConfig, importDescriptions: descriptions);
        var source = new RaylibInteropEmitterAdapter().Emit(new EmitRequest(
            manifestBytes, manifestPath, manifestSha256,
            new EmitTarget("Raylib6Native", EmitStrategy.LibraryImport, "Interop/Raylib6Native.g.cs", "Novolis.Raylib.Interop", "Novolis.Raylib.Bindings"),
            context));

        WriteUnit(source, context, hooks);
        Console.WriteLine($"Wrote {RaylibManifestModels.LoadImports(manifestPath).Count} imports to {outPath}");
    }

    private static void EmitSimple(
        BindingCodegenOptions options,
        string manifestPath,
        RaylibCodegenPhase phase,
        IBindingEmitter emitter,
        DebugConfigFragment? debugConfig,
        IReadOnlyList<IRaylibCodegenHook> hooks)
    {
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = StepFileFingerprint.Sha256Hex(manifestBytes);
        var outPath = phase switch
        {
            RaylibCodegenPhase.ImGui => Path.Combine(RepoPaths.InteropDir(options.RepoRoot), "ImguiShimExports.g.cs"),
            RaylibCodegenPhase.Debug => Path.Combine(RepoPaths.InteropDir(options.RepoRoot), "RaylibDebugFrameHooks.g.cs"),
            _ => throw new InvalidOperationException($"Unexpected phase {phase}"),
        };

        var context = CreateContext(options, phase, outPath, manifestPath, manifestSha256, debugConfig);
        var source = emitter.Emit(new EmitRequest(
            manifestBytes, manifestPath, manifestSha256,
            new EmitTarget("", emitter.Strategy, "", "Novolis.Raylib.Interop", "Novolis.Raylib.Bindings"),
            context));

        WriteUnit(source, context, hooks);
        Console.WriteLine($"Wrote {outPath}");
    }

    private static void EmitRayguiInterop(
        BindingCodegenOptions options,
        string manifestPath,
        DebugConfigFragment? debugConfig,
        IReadOnlyList<IRaylibCodegenHook> hooks)
    {
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = StepFileFingerprint.Sha256Hex(manifestBytes);
        var outPath = Path.Combine(options.RepoRoot, "src", "Novolis.Raylib.Raygui", "Interop", "RayguiShimExports.g.cs");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var context = CreateContext(options, RaylibCodegenPhase.Raygui, outPath, manifestPath, manifestSha256, debugConfig);
        var source = new RayguiInteropEmitterAdapter().Emit(new EmitRequest(
            manifestBytes, manifestPath, manifestSha256,
            new EmitTarget("RayguiShimExports", EmitStrategy.DynamicExports, "Interop/RayguiShimExports.g.cs", "Novolis.Raylib.Interop", "Novolis.Raylib.Raygui"),
            context));

        WriteUnit(source, context, hooks);
        Console.WriteLine($"Wrote {RayguiManifestModels.LoadFunctions(manifestPath).Count} exports to {outPath}");
    }

    private static void EmitManifestTypesInternal(
        BindingCodegenOptions options,
        string manifestPath,
        string root,
        DebugConfigFragment? debugConfig,
        string? facadeMethodImpl,
        (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) headerDocs,
        IReadOnlyList<IRaylibCodegenHook> hooks,
        string label)
    {
        var manifestBytes = File.ReadAllBytes(manifestPath);
        var manifestSha256 = StepFileFingerprint.Sha256Hex(manifestBytes);
        var types = FacadeManifestModels.LoadTypes(manifestPath);

        foreach (var facadeType in types)
        {
            var outPath = Path.Combine(root, facadeType.Folder, $"{facadeType.Name}.g.cs");
            var adapter = new FacadeEmitterAdapter(facadeType, headerDocs.Raylib, headerDocs.Raygui, facadeMethodImpl);
            var context = CreateContext(options, RaylibCodegenPhase.Facade, outPath, manifestPath, manifestSha256, debugConfig, facadeType.Name, facadeMethodImpl);
            var source = adapter.Emit(new EmitRequest(
                manifestBytes, manifestPath, manifestSha256,
                new EmitTarget(facadeType.Name, EmitStrategy.FacadeForward, outPath, facadeType.Namespace, "Novolis.Raylib.Runtime"),
                context));

            WriteUnit(source, context, hooks, FormatPolicy.NormalizeWhitespace);
            Console.WriteLine($"Wrote {label} {facadeType.Name} to {outPath}");
        }
    }

    private static RaylibCodegenContext CreateContext(
        BindingCodegenOptions options,
        RaylibCodegenPhase phase,
        string outputPath,
        string manifestPath,
        string manifestSha256,
        DebugConfigFragment? debugConfig,
        string? facadeTypeName = null,
        string? facadeMethodImpl = null,
        IReadOnlyDictionary<string, string>? importDescriptions = null) =>
        new()
        {
            RepoRoot = options.RepoRoot,
            Phase = phase,
            OutputPath = outputPath,
            ManifestPath = manifestPath,
            ManifestSha256 = manifestSha256,
            RegenerateHint = options.RegenerateHint,
            DebugConfig = debugConfig,
            FacadeTypeName = facadeTypeName,
            FacadeMethodImpl = facadeMethodImpl,
            ImportDescriptions = importDescriptions ?? new Dictionary<string, string>(StringComparer.Ordinal),
        };

    private static void WriteUnit(
        string source,
        RaylibCodegenContext context,
        IReadOnlyList<IRaylibCodegenHook> hooks,
        FormatPolicy? format = null)
    {
        var policy = format ?? (context.Phase == RaylibCodegenPhase.Facade
            ? FormatPolicy.NormalizeWhitespace
            : FormatPolicy.RoslynFormatter);

        RoslynEmitWriter<RaylibCodegenPhase, RaylibCodegenContext>.WriteFile(
            source,
            context,
            context.Phase,
            hooks,
            policy);
    }
}

internal static class RaylibHookDiscovery
{
    public static IReadOnlyList<IRaylibCodegenHook> DiscoverAll()
    {
        var assemblies = new List<Assembly> { typeof(RaylibHookDiscovery).Assembly };

        var hooksName = "Novolis.Raylib.CodeGen.Hooks";
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, hooksName, StringComparison.Ordinal));
        if (loaded is not null)
        {
            assemblies.Add(loaded);
        }
        else
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var path in new[]
                     {
                         Path.Combine(baseDir, $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Debug", "net10.0", $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Release", "net10.0", $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "..", "codegen", "Novolis.Raylib.CodeGen.Hooks", "bin", "Debug", "net10.0", $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "..", "codegen", "Novolis.Raylib.CodeGen.Hooks", "bin", "Release", "net10.0", $"{hooksName}.dll"),
                     })
            {
                var full = Path.GetFullPath(path);
                if (!File.Exists(full))
                    continue;

                assemblies.Add(Assembly.LoadFrom(full));
                break;
            }
        }

        return HookDiscovery.Discover<RaylibCodegenPhase, RaylibCodegenContext>(assemblies.ToArray())
            .OfType<IRaylibCodegenHook>()
            .ToList();
    }
}
