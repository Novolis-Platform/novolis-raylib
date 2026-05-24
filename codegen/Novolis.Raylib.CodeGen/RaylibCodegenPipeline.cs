using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen;

public sealed class RaylibCodegenPipeline
{
    private readonly RaylibBindingCodegenHost _host;
    private readonly string _repoRoot;

    public RaylibCodegenPipeline(string repoRoot, IReadOnlyList<IRaylibCodegenHook>? hooks = null)
    {
        _repoRoot = repoRoot;
        _host = new RaylibBindingCodegenHost(RaylibBindingManifestSource.Instance, hooks);
    }

    public int GenerateAll()
    {
        return _host.GenerateAll(CreateOptions(verifyManifest: true));
    }

    public void GenerateBindingsOnly(TextWriter? log = null)
    {
        _host.GenerateBindingsOnly(CreateOptions(verifyManifest: false), log);
    }

    private BindingCodegenOptions CreateOptions(bool verifyManifest) =>
        new()
        {
            Environment = CodegenEnvironment.Physical(_repoRoot),
            Manifests = RaylibBindingManifestSource.Instance,
            IncludeRaygui = true,
            VerifyManifest = verifyManifest,
            RegenerateHint = CodegenHeaders.RegenerateHint,
        };
}
