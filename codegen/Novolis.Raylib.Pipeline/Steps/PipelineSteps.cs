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

internal sealed class DriftStep : IPipelineStep
{
    public string Id => "step_07_drift";

    public string Description => "Assert no drift in manifests and generated C#.";

    public IReadOnlyList<string> DependsOn => ["step_06_codegen"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) => [];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            "codegen/Novolis.Raylib.Manifests/",
            "src/Novolis.Raylib.Bindings/",
            "src/Novolis.Raylib.Runtime/",
            "src/Novolis.Raylib.Raygui/",
        };

        var args = string.Join(' ', paths.Select(p => $"\"{p}\""));
        var code = await ProcessRunner.RunAsync(
            context,
            "git",
            $"diff --exit-code {args}",
            context.RepoRoot,
            cancellationToken);

        if (code != 0)
        {
            return new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = "git diff detected drift in C# manifests or generated C#" },
            };
        }

        await context.Log.WriteLineAsync("drift check: OK");
        return new StepExecutionResult { Status = StepStatus.Succeeded };
    }
}

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

internal static class CodegenOutputCatalog
{
    public static IReadOnlyList<string> AllGeneratedFiles(string repoRoot)
    {
        var list = new List<string>();
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Bindings", "Interop"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Rendering"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Windowing"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Interact"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Timing"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Audio"), "*.g.cs"));
        list.AddRange(Directory.GetFiles(Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Hud"), "*.g.cs"));
        var gui = Path.Combine(repoRoot, "src", "Novolis.Raylib.Runtime", "Gui", "Gui.g.cs");
        if (File.Exists(gui))
            list.Add(gui);
        var rayguiInterop = Path.Combine(repoRoot, "src", "Novolis.Raylib.Raygui", "Interop", "RayguiShimExports.g.cs");
        if (File.Exists(rayguiInterop))
            list.Add(rayguiInterop);
        var raygui = Path.Combine(repoRoot, "src", "Novolis.Raylib.Raygui", "RayGui", "RayGui.g.cs");
        if (File.Exists(raygui))
            list.Add(raygui);
        return list;
    }
}
