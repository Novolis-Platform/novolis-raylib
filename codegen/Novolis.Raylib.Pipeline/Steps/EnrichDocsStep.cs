using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class EnrichDocsStep : IPipelineStep
{
    public string Id => "step_04_enrich_docs";

    public string Description => "Enrich façade manifest docs from headers.";

    public IReadOnlyList<string> DependsOn => ["step_01_source"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
    [
        PipelinePaths.RaylibHeaderPath(context.RepoRoot),
        PipelinePaths.RayguiHeaderPath(context.RepoRoot),
        ..RaylibManifestInputPaths.AllManifestSourceFiles(context.RepoRoot),
    ];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = FacadeDocEnricher.Enrich(context.RepoRoot, write: true);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"enrich-docs failed with exit code {code}" },
            });
        }

        context.Log.WriteLine("enrich-docs: OK");
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}
