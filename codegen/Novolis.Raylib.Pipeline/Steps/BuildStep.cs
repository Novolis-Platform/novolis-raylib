using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.Pipeline.Steps;

internal sealed class BuildStep : IPipelineStep
{
    public string Id => "step_08_build";

    public string Description => "Release build Bindings and Runtime.";

    public IReadOnlyList<string> DependsOn => ["step_07_drift"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) => [];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        foreach (var project in new[]
                 {
                     "src/Novolis.Raylib.Bindings/Novolis.Raylib.Bindings.csproj",
                     "src/Novolis.Raylib.Runtime/Novolis.Raylib.Runtime.csproj",
                 })
        {
            var code = await ProcessRunner.RunAsync(
                context,
                "dotnet",
                $"build \"{project}\" -c Release",
                context.RepoRoot,
                cancellationToken);
            if (code != 0)
            {
                return new StepExecutionResult
                {
                    Status = StepStatus.Failed,
                    Error = new StepErrorRecord { Message = $"dotnet build failed for {project} (exit {code})" },
                };
            }
        }

        return new StepExecutionResult { Status = StepStatus.Succeeded };
    }
}
