using Novolis.CodeGen.Pipeline;

namespace Novolis.Raylib.CodeGen;

public sealed class RaylibPipelineLayout : IPipelineLayout
{
    public RaylibPipelineLayout(string repoRoot) => RepoRoot = repoRoot;

    public string RepoRoot { get; }

    public string StepsRoot => PipelinePaths.StepsRoot(RepoRoot);

    public string ManifestDir => PipelinePaths.PipelineRaylibDir(RepoRoot);

    public string StepDir(string stepId) => PipelinePaths.StepDir(RepoRoot, stepId);

    public string StepArtifactsDir(string stepId) => PipelinePaths.StepArtifactsDir(RepoRoot, stepId);

    public static RaylibPipelineLayout Find() => new(PipelinePaths.FindRepoRoot());
}
