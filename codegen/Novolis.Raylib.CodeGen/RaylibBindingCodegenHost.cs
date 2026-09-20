using System.Reflection;
using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibDebugHooksEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.DebugHooks;

    public string Emit(EmitRequest request) =>
        RaylibDebugHooksEmitter.Emit((DebugConfigFragment)request.Fragment, request.ManifestSha256);
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
            var verify = RaylibManifestVerifier.Verify(options.Environment, options.Manifests);
            if (verify != 0)
                return verify;
        }

        GenerateBindingsOnly(options, log);
        return 0;
    }

    public void GenerateBindingsOnly(BindingCodegenOptions options, TextWriter? log = null)
    {
        var interop = _manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "raylib6");
        var debug = _manifests.TryGet<DebugConfigFragment>(FragmentKind.DebugConfig, "raylib-debug");
        var headerDocs = LoadHeaderDocs(options.Environment);
        var documentation = new RaylibFacadeDocumentationResolver(headerDocs.Raylib, headerDocs.Raygui);
        var run = new BindingCodegenRun<RaylibCodegenPhase, RaylibCodegenContext>
        {
            Project = BuildProject(interop.Policy, documentation),
            Options = options,
            Hooks = _hooks,
            SelectPhase = SelectPhase,
            CreateContext = (job, fragment, outputPath, fingerprint) => new RaylibCodegenContext
            {
                Environment = options.Environment,
                Phase = SelectPhase(job),
                OutputPath = outputPath,
                Fragment = fragment,
                ManifestSha256 = fingerprint,
                RegenerateHint = options.RegenerateHint,
                DebugConfig = debug,
                FacadeTypeName = job.Target.Strategy == EmitStrategy.FacadeForward ? job.Target.ClassName : null,
                FacadeMethodImpl = interop.Policy.FacadeMethodImpl,
                ImportDescriptions = ImportDescriptions(interop),
            },
        };

        new BindingCodegenHost<RaylibCodegenPhase, RaylibCodegenContext>().Generate(run, log);
    }

    private BindingProject BuildProject(
        InteropPolicySpec policy,
        IFacadeDocumentationResolver documentation)
    {
        var project = BindingProject.Create("Novolis.Raylib")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/Utf8StringMarshaller.cs", "UTF-8 marshalling")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibColor.cs", "Raylib color type")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibInteropMarshaling.cs", "Interop helpers")
            .RequireCompanion("src/Novolis.Raylib.Bindings/Interop/RaylibDebugCaptureGate.cs", "Debug capture gate")
            .RequireCompanion("src/Novolis.Raylib.Runtime/ImguiShimHost.cs", "ImGui native host")
            .RequireCompanion("src/Novolis.Raylib.Runtime/Gui/GuiControls.cs", "ImGui controls layer")
            .RequireCompanion("src/Novolis.Raylib.Raygui/RayGuiControls.cs", "Raygui controls layer")
            .AddJob(
                new BindingEmitJob(
                    "raylib interop",
                    FragmentKind.InteropExports,
                    "raylib6",
                    new LibraryImportEmitter(),
                    new EmitTarget(
                        "Raylib6Native",
                        EmitStrategy.LibraryImport,
                        "src/Novolis.Raylib.Bindings/Interop/Raylib6Native.g.cs",
                        "Novolis.Raylib.Interop",
                        "Novolis.Raylib.Bindings",
                        LibraryConstantName: "RaylibDll",
                        TypeSummary: "Low-level raylib 6 entry points (manifest-generated <c>[LibraryImport]</c>).",
                        StructSummary: "Blittable layout for raylib C struct (generated from manifest).")))
            .AddJob(
                new BindingEmitJob(
                    "imgui interop",
                    FragmentKind.ShimExports,
                    "imgui",
                    new DynamicExportsEmitter(),
                    new EmitTarget(
                        "ImguiShimExports",
                        EmitStrategy.DynamicExports,
                        "src/Novolis.Raylib.Bindings/Interop/ImguiShimExports.g.cs",
                        "Novolis.Raylib.Interop",
                        "Novolis.Raylib.Bindings")))
            .AddJob(
                new BindingEmitJob(
                    "raygui interop",
                    FragmentKind.ShimExports,
                    "raygui",
                    new DynamicExportsEmitter(),
                    new EmitTarget(
                        "RayguiShimExports",
                        EmitStrategy.DynamicExports,
                        "src/Novolis.Raylib.Raygui/Interop/RayguiShimExports.g.cs",
                        "Novolis.Raylib.Interop",
                        "Novolis.Raylib.Raygui"),
                    Optional: true))
            .AddJob(
                new BindingEmitJob(
                    "raylib debug hooks",
                    FragmentKind.DebugConfig,
                    "raylib-debug",
                    new RaylibDebugHooksEmitterAdapter(),
                    new EmitTarget(
                        "RaylibDebugFrameHooks",
                        EmitStrategy.DebugHooks,
                        "src/Novolis.Raylib.Bindings/Interop/RaylibDebugFrameHooks.g.cs",
                        "Novolis.Raylib.Interop",
                        "Novolis.Raylib.Bindings")));

        AddFacadeJobs(
            project,
            "facades",
            "src/Novolis.Raylib.Runtime",
            "Novolis.Raylib.Runtime",
            policy.FacadeMethodImpl,
            documentation);
        AddFacadeJobs(
            project,
            "hud",
            "src/Novolis.Raylib.Runtime",
            "Novolis.Raylib.Runtime",
            policy.FacadeMethodImpl,
            documentation);
        AddFacadeJobs(
            project,
            "gui",
            "src/Novolis.Raylib.Runtime",
            "Novolis.Raylib.Runtime",
            policy.FacadeMethodImpl,
            documentation);
        AddFacadeJobs(
            project,
            "raygui",
            "src/Novolis.Raylib.Raygui",
            "Novolis.Raylib.Raygui",
            policy.FacadeMethodImpl,
            documentation,
            optional: true);
        return project;
    }

    private void AddFacadeJobs(
        BindingProject project,
        string fragmentId,
        string root,
        string assemblyName,
        string? facadeMethodImpl,
        IFacadeDocumentationResolver documentation,
        bool optional = false)
    {
        var fragment = _manifests.GetRequired<FacadeTypesFragment>(FragmentKind.FacadeTypes, fragmentId);
        foreach (var type in fragment.Types)
        {
            project.AddJob(
                new BindingEmitJob(
                    $"{fragmentId} {type.Name}",
                    FragmentKind.FacadeTypes,
                    fragmentId,
                    new FacadeForwardEmitter(documentation),
                    new EmitTarget(
                        type.Name,
                        EmitStrategy.FacadeForward,
                        Path.Combine(root, type.Folder, $"{type.Name}.g.cs"),
                        type.Namespace,
                        assemblyName,
                        FacadeMethodImpl: facadeMethodImpl),
                    Optional: optional,
                    FormatPolicy: BindingFormatPolicy.NormalizeWhitespace,
                    Slice: type.Name));
        }
    }

    private static RaylibCodegenPhase SelectPhase(BindingEmitJob job) =>
        job.Target.Strategy switch
        {
            EmitStrategy.LibraryImport => RaylibCodegenPhase.Interop,
            EmitStrategy.DynamicExports when job.FragmentId == "imgui" => RaylibCodegenPhase.ImGui,
            EmitStrategy.DynamicExports when job.FragmentId == "raygui" => RaylibCodegenPhase.Raygui,
            EmitStrategy.DebugHooks => RaylibCodegenPhase.Debug,
            EmitStrategy.FacadeForward => RaylibCodegenPhase.Facade,
            _ => throw new InvalidOperationException($"No Raylib phase exists for job '{job.Label}'."),
        };

    private static IReadOnlyDictionary<string, string> ImportDescriptions(InteropExportsFragment fragment) =>
        fragment.Imports
            .Where(import => !string.IsNullOrWhiteSpace(import.Description))
            .ToDictionary(import => import.Name, import => import.Description!, StringComparer.Ordinal);

    private static (IReadOnlyDictionary<string, string> Raylib, IReadOnlyDictionary<string, string> Raygui) LoadHeaderDocs(
        CodegenEnvironment environment) =>
        (
            RaylibHeaderDocs.Load(environment, RaylibHeaderDocs.RaylibHeaderPath(environment.RepoRoot)),
            RaylibHeaderDocs.Load(environment, RaylibHeaderDocs.RayguiHeaderPath(environment.RepoRoot)));
}

internal sealed class RaylibFacadeDocumentationResolver(
    IReadOnlyDictionary<string, string> raylibComments,
    IReadOnlyDictionary<string, string> rayguiComments) : IFacadeDocumentationResolver
{
    public string? ResolveTypeSummary(FacadeTypeSpec type) =>
        FacadeDocResolver.ResolveTypeSummary(type);

    public string? ResolveMethodSummary(FacadeTypeSpec type, FacadeMethodSpec method) =>
        FacadeDocResolver.ResolveMethodSummary(
            type,
            method,
            raylibComments,
            rayguiComments);
}

internal static class RaylibHookDiscovery
{
    public static IReadOnlyList<IRaylibCodegenHook> DiscoverAll()
    {
        var assemblies = new List<Assembly> { typeof(RaylibHookDiscovery).Assembly };
        var hooksName = "Novolis.Raylib.CodeGen.Hooks";
        var loaded = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, hooksName, StringComparison.Ordinal));
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
