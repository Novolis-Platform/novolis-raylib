using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class VerifyDocsStep : IPipelineStep
{
    public string Id => "step_05_verify_docs";

    public string Description => "Verify façade manifest documentation.";

    public IReadOnlyList<string> DependsOn => ["step_04_enrich_docs"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        new EnrichDocsStep().InputPaths(context);

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = FacadeDocVerifier.Verify(context.RepoRoot);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-docs failed with exit code {code}" },
            });
        }

        context.Log.WriteLine("verify-docs: OK");
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}
