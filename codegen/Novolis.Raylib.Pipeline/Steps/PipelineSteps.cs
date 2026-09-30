using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class VerifyManifestStep : IPipelineStep
{
    public string Id => "step_03_verify_manifest";

    public string Description => "Verify raylib manifest imports against vendor raylib.h.";

    public IReadOnlyList<string> DependsOn => ["step_01_source"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
    [
        PipelinePaths.VersionsJson(context.RepoRoot),
        ..RaylibManifestInputPaths.AllManifestSourceFiles(context.RepoRoot),
        PipelinePaths.RaylibHeaderPath(context.RepoRoot),
    ];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = RaylibManifestVerifier.Verify(context.RepoRoot);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-raylib-manifest failed with exit code {code}" },
            });
        }

        context.Log.WriteLine("verify-raylib-manifest: OK");
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}
