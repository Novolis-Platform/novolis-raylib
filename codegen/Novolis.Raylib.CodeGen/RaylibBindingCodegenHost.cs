using System.Reflection;
using System.IO.Abstractions;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.CodeGen.Pipeline;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.LibraryImport;

    public string Emit(EmitRequest request)
    {
        var fragment = (InteropExportsFragment)request.Fragment;
        return RaylibInteropEmitter.Emit(fragment, request.ManifestSha256, RaylibManifestMapping.ToPolicy(fragment.Policy));
    }
}

internal sealed class ImguiInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DynamicExports;

    public string Emit(EmitRequest request) =>
        ImguiInteropEmitter.Emit((ShimExportsFragment)request.Fragment, request.ManifestSha256);
}

internal sealed class RayguiInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DynamicExports;

    public string Emit(EmitRequest request) =>
        RayguiInteropEmitter.Emit((ShimExportsFragment)request.Fragment, request.ManifestSha256);
}

internal sealed class RaylibDebugHooksEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DebugHooks;

    public string Emit(EmitRequest request) =>
        RaylibDebugHooksEmitter.Emit((DebugConfigFragment)request.Fragment, request.ManifestSha256);
}

internal sealed class FacadeEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.FacadeForward;

    private readonly FacadeTypeSpec _type;
    private readonly IReadOnlyDictionary<string, string> _raylibComments;
    private readonly IReadOnlyDictionary<string, string> _rayguiComments;
    private readonly string? _facadeMethodImpl;

    public FacadeEmitterAdapter(
        FacadeTypeSpec type,
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
        FacadeEmitter.EmitType(
            RaylibManifestMapping.ToFacadeType(_type),
            request.ManifestSha256,
            _raylibComments,
            _rayguiComments,
            _facadeMethodImpl);
}

public sealed class RaylibBindingCodegenHost : IBindingCodegenHost
{
    private readonly IBindingManifestSource _manifests;
    private readonly IReadOnlyList<IRaylibCodegenHook> _hooks;

    public RaylibBindingCodegenHost(
        IBindingManifestSource? manifests = null,
        IReadOnlyList<IRaylibCodegenHook>? hooks = null)
    {
        _manifests = manifests ?? RaylibBindingManifestSource.Instance;
        _hooks = hooks ?? RaylibHookDiscovery.DiscoverAll();
    }

    public int GenerateAll(BindingCodegenOptions options, TextWriter? log = null)
    {
        if (options.VerifyManifest)
        {
            var verify = RaylibManifestVerifier.Verify(options.Environment);
            if (verify != 0)
                return verify;
        }

        GenerateBindingsOnly(options, log);
        return 0;
    }

    public void GenerateBindingsOnly(BindingCodegenOptions options, TextWriter? log = null)
    {
        var env = options.Environment;
        BindingCodegenExecutor.ValidateCompanions(BuildProject(), env);

        var interop = _manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "raylib6");
        var debug = _manifests.TryGet<DebugConfigFragment>(FragmentKind.DebugConfig, "raylib-debug");
        var policy = RaylibManifestMapping.ToPolicy(interop.Policy);
        var importDescriptions = RaylibManifestMapping.ImportDescriptions(interop);
        var headerDocs = LoadHeaderDocs(env);

        EmitInterop(env, options, interop, debug, importDescriptions, policy, log);
        EmitShim(env, options, RaylibCodegenPhase.ImGui, "imgui", new ImguiInteropEmitterAdapter(), debug, log);
        if (options.IncludeRaygui && _manifests.TryGet<ShimExportsFragment>(FragmentKind.ShimExports, "raygui") is { } rayguiShim)
            EmitShimFragment(env, options, RaylibCodegenPhase.Raygui, rayguiShim, new RayguiInteropEmitterAdapter(), debug, log);
        else if (!options.IncludeRaygui)
            log?.WriteLine("skip: raygui interop (IncludeRaygui=false)");

        if (debug is not null)
            EmitDebug(env, options, debug, log);

        EmitFacadeManifest(env, options, "facades", RepoPaths.RuntimeDir(env.RepoRoot), debug, policy.FacadeMethodImpl, headerDocs, log);
        EmitFacadeManifest(env, options, "hud", RepoPaths.RuntimeDir(env.RepoRoot), debug, policy.FacadeMethodImpl, headerDocs, log);
        EmitFacadeManifest(env, options, "gui", RepoPaths.RuntimeDir(env.RepoRoot), debug, policy.FacadeMethodImpl, headerDocs, log);
        if (options.IncludeRaygui && _manifests.TryGet<FacadeTypesFragment>(FragmentKind.FacadeTypes, "raygui") is { } rayguiFacade)
            EmitFacadeFragment(env, options, rayguiFacade, Path.Combine(env.RepoRoot, "src", "Novolis.Raylib.Raygui"), debug, policy.FacadeMethodImpl, headerDocs, log);
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

    private void EmitInterop(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        InteropExportsFragment fragment,
        DebugConfigFragment? debug,
        IReadOnlyDictionary<string, string> importDescriptions,
        RaylibInteropPolicy policy,
        TextWriter? log)
    {
        log?.WriteLine("emit: raylib interop");
        var sha = fragment.Sha256Hex();
        var outPath = Path.Combine(RepoPaths.InteropDir(env.RepoRoot), "Raylib6Native.g.cs");
        var context = CreateContext(env, options, RaylibCodegenPhase.Interop, outPath, fragment, sha, debug, importDescriptions: importDescriptions, facadeMethodImpl: policy.FacadeMethodImpl);
        var source = new RaylibInteropEmitterAdapter().Emit(new EmitRequest(fragment, sha, new EmitTarget("Raylib6Native", EmitStrategy.LibraryImport, "Interop/Raylib6Native.g.cs", "Novolis.Raylib.Interop", "Novolis.Raylib.Bindings"), context));
        WriteUnit(source, context);
        Console.WriteLine($"Wrote {fragment.Imports.Count} imports to {outPath}");
    }

    private void EmitShim(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        RaylibCodegenPhase phase,
        string fragmentId,
        IBindingEmitter emitter,
        DebugConfigFragment? debug,
        TextWriter? log)
    {
        var fragment = _manifests.GetRequired<ShimExportsFragment>(FragmentKind.ShimExports, fragmentId);
        EmitShimFragment(env, options, phase, fragment, emitter, debug, log);
    }

    private void EmitShimFragment(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        RaylibCodegenPhase phase,
        ShimExportsFragment fragment,
        IBindingEmitter emitter,
        DebugConfigFragment? debug,
        TextWriter? log)
    {
        log?.WriteLine($"emit: {fragment.Id} interop");
        var sha = fragment.Sha256Hex();
        var outPath = phase switch
        {
            RaylibCodegenPhase.ImGui => Path.Combine(RepoPaths.InteropDir(env.RepoRoot), "ImguiShimExports.g.cs"),
            RaylibCodegenPhase.Raygui => Path.Combine(env.RepoRoot, "src", "Novolis.Raylib.Raygui", "Interop", "RayguiShimExports.g.cs"),
            _ => throw new InvalidOperationException($"Unexpected phase {phase}"),
        };

        var context = CreateContext(env, options, phase, outPath, fragment, sha, debug);
        var source = emitter.Emit(new EmitRequest(fragment, sha, new EmitTarget("", emitter.Strategy, "", "Novolis.Raylib.Interop", "Novolis.Raylib.Bindings"), context));
        WriteUnit(source, context);
        Console.WriteLine($"Wrote {outPath}");
    }

    private void EmitDebug(CodegenEnvironment env, BindingCodegenOptions options, DebugConfigFragment fragment, TextWriter? log)
    {
        log?.WriteLine("emit: debug hooks");
        var sha = fragment.Sha256Hex();
        var outPath = Path.Combine(RepoPaths.InteropDir(env.RepoRoot), "RaylibDebugFrameHooks.g.cs");
        var context = CreateContext(env, options, RaylibCodegenPhase.Debug, outPath, fragment, sha, fragment);
        var source = new RaylibDebugHooksEmitterAdapter().Emit(new EmitRequest(fragment, sha, new EmitTarget("", EmitStrategy.DebugHooks, "", "Novolis.Raylib.Interop", "Novolis.Raylib.Bindings"), context));
        WriteUnit(source, context);
        Console.WriteLine($"Wrote {outPath}");
    }

    private void EmitFacadeManifest(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        string fragmentId,
        string root,
        DebugConfigFragment? debug,
        string? facadeMethodImpl,
        (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) headerDocs,
        TextWriter? log)
    {
        var fragment = _manifests.GetRequired<FacadeTypesFragment>(FragmentKind.FacadeTypes, fragmentId);
        EmitFacadeFragment(env, options, fragment, root, debug, facadeMethodImpl, headerDocs, log);
    }

    private void EmitFacadeFragment(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        FacadeTypesFragment fragment,
        string root,
        DebugConfigFragment? debug,
        string? facadeMethodImpl,
        (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) headerDocs,
        TextWriter? log)
    {
        log?.WriteLine($"emit: {fragment.Id}");
        var sha = fragment.Sha256Hex();
        foreach (var facadeType in fragment.Types)
        {
            var outPath = Path.Combine(root, facadeType.Folder, $"{facadeType.Name}.g.cs");
            var adapter = new FacadeEmitterAdapter(facadeType, headerDocs.Raylib, headerDocs.Raygui, facadeMethodImpl);
            var context = CreateContext(env, options, RaylibCodegenPhase.Facade, outPath, fragment, sha, debug, facadeType.Name, facadeMethodImpl);
            var source = adapter.Emit(new EmitRequest(fragment, sha, new EmitTarget(facadeType.Name, EmitStrategy.FacadeForward, outPath, facadeType.Namespace, "Novolis.Raylib.Runtime"), context));
            WriteUnit(source, context, FormatPolicy.NormalizeWhitespace);
            Console.WriteLine($"Wrote {fragment.Id} {facadeType.Name} to {outPath}");
        }
    }

    private static (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) LoadHeaderDocs(
        CodegenEnvironment env) =>
        (
            RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RaylibHeaderPath(env.RepoRoot)),
            RaylibHeaderDocs.Load(env, RaylibHeaderDocs.RayguiHeaderPath(env.RepoRoot)));

    private static RaylibCodegenContext CreateContext(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        RaylibCodegenPhase phase,
        string outputPath,
        IManifestFragment fragment,
        string manifestSha256,
        DebugConfigFragment? debugConfig,
        string? facadeTypeName = null,
        string? facadeMethodImpl = null,
        IReadOnlyDictionary<string, string>? importDescriptions = null) =>
        new()
        {
            Environment = env,
            Phase = phase,
            OutputPath = outputPath,
            Fragment = fragment,
            ManifestSha256 = manifestSha256,
            RegenerateHint = options.RegenerateHint,
            DebugConfig = debugConfig,
            FacadeTypeName = facadeTypeName,
            FacadeMethodImpl = facadeMethodImpl,
            ImportDescriptions = importDescriptions ?? new Dictionary<string, string>(StringComparer.Ordinal),
        };

    private void WriteUnit(string source, RaylibCodegenContext context, FormatPolicy? format = null)
    {
        var policy = format ?? (context.Phase == RaylibCodegenPhase.Facade
            ? FormatPolicy.NormalizeWhitespace
            : FormatPolicy.RoslynFormatter);

        RoslynEmitWriter<RaylibCodegenPhase, RaylibCodegenContext>.WriteFile(
            source,
            context,
            context.Phase,
            _hooks,
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
            assemblies.Add(loaded);
        else
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var path in new[]
                     {
                         Path.Combine(baseDir, $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Debug", "net10.0", $"{hooksName}.dll"),
                         Path.Combine(baseDir, "..", "Novolis.Raylib.CodeGen.Hooks", "bin", "Release", "net10.0", $"{hooksName}.dll"),
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
