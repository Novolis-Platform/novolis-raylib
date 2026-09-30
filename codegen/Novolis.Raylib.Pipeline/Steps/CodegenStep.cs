using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class CodegenStep : IPipelineStep
{
    public string Id => "step_06_codegen";

    public string Description => "Generate interop and façade *.g.cs files.";

    public IReadOnlyList<string> DependsOn => ["step_03_verify_manifest"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        RaylibManifestInputPaths.AllManifestSourceFiles(context.RepoRoot);

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
        CodegenOutputCatalog.AllGeneratedFiles(context.RepoRoot);

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var verify = RaylibManifestVerifier.Verify(context.RepoRoot);
        if (verify != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-raylib-manifest failed with exit code {verify}" },
            });
        }

        var pipeline = new RaylibCodegenPipeline(context.RepoRoot);
        pipeline.GenerateBindingsOnly(context.Log);
        var outputs = StepFileFingerprint.DescribeOutputs(ExpectedOutputPaths(context), context.RepoRoot);
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
            Outputs = outputs,
        });
    }
}
