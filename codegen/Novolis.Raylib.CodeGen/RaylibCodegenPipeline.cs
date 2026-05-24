using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.CodeGen;

public sealed class RaylibCodegenPipeline
{
    private readonly RaylibBindingCodegenHost _host;
    private readonly string _repoRoot;

    public RaylibCodegenPipeline(string repoRoot, IReadOnlyList<IRaylibCodegenHook>? hooks = null)
    {
        _repoRoot = repoRoot;
        _host = new RaylibBindingCodegenHost(hooks);
    }

    public int GenerateAll()
    {
        return _host.GenerateAll(new BindingCodegenOptions
        {
            RepoRoot = _repoRoot,
            IncludeRaygui = ManifestExists("raygui-exports.manifest.json"),
            VerifyManifest = true,
            RegenerateHint = CodegenHeaders.RegenerateHint,
        });
    }

    public void GenerateBindingsOnly(TextWriter? log = null)
    {
        _host.GenerateBindingsOnly(new BindingCodegenOptions
        {
            RepoRoot = _repoRoot,
            IncludeRaygui = ManifestExists("raygui-exports.manifest.json"),
            VerifyManifest = false,
            RegenerateHint = CodegenHeaders.RegenerateHint,
        }, log);
    }

    private bool ManifestExists(string fileName) =>
        File.Exists(Path.Combine(RepoPaths.PipelineDir(_repoRoot), fileName));
}
