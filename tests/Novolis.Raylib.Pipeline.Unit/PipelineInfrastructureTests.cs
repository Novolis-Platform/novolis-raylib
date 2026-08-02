using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineStepRegistryTests
{
    [Test]
    public async Task CreateAll_returns_all_nine_steps_in_order()
    {
        var steps = PipelineStepRegistry.CreateAll();
        await Assert.That(steps.Count).IsEqualTo(9);
        await Assert.That(steps[0].Id).IsEqualTo("step_01_source");
        await Assert.That(steps[1].Id).IsEqualTo("step_02_native");
        await Assert.That(steps[2].Id).IsEqualTo("step_02a_android");
        await Assert.That(steps[3].Id).IsEqualTo("step_03_verify_manifest");
        await Assert.That(steps[8].Id).IsEqualTo("step_08_build");
    }

    [Test]
    public async Task CreateAll_step_ids_are_unique()
    {
        var ids = PipelineStepRegistry.CreateAll().Select(s => s.Id).ToList();
        await Assert.That(ids.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(ids.Count);
    }
}

public sealed class ProcessRunnerTests
{
    [Test]
    public async Task RunAsync_returns_process_exit_code_and_logs_command()
    {
        var tempRoot = CreateTempRepoRoot();
        try
        {
            var log = new StringWriter();
            var layout = new RaylibPipelineLayout(tempRoot);
            var context = new PipelineContext { Layout = layout, Log = log, Force = false };

            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c exit 42")
                : ("/bin/sh", "-c \"exit 42\"");

            var code = await ProcessRunner.RunAsync(context, fileName, arguments, tempRoot, CancellationToken.None);
            await Assert.That(code).IsEqualTo(42);
            await Assert.That(log.ToString()).Contains(fileName);
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    [Test]
    public async Task RunAsync_captures_stdout()
    {
        var tempRoot = CreateTempRepoRoot();
        try
        {
            var log = new StringWriter();
            var layout = new RaylibPipelineLayout(tempRoot);
            var context = new PipelineContext { Layout = layout, Log = log, Force = false };

            var (fileName, arguments) = OperatingSystem.IsWindows()
                ? ("cmd.exe", "/c echo pipeline-stdout")
                : ("/bin/sh", "-c \"echo pipeline-stdout\"");

            var code = await ProcessRunner.RunAsync(context, fileName, arguments, tempRoot, CancellationToken.None);
            await Assert.That(code).IsEqualTo(0);
            await Assert.That(log.ToString()).Contains("pipeline-stdout");
        }
        finally
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }

    private static string CreateTempRepoRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "novolis-pipeline-unit", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        File.WriteAllText(Path.Combine(tempRoot, "Directory.Packages.props"), "<Project/>");
        return tempRoot;
    }
}

public sealed class PipelineProgramTests
{
    [Test]
    public async Task Main_list_returns_zero()
    {
        var code = await Program.Main(["list"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_help_flag_returns_zero()
    {
        var code = await Program.Main(["-h"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_no_args_returns_one()
    {
        var code = await Program.Main([]);
        await Assert.That(code).IsEqualTo(1);
    }

    [Test]
    public async Task Main_explain_known_step_returns_zero()
    {
        var code = await Program.Main(["explain", "step_06_codegen"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_explain_unknown_step_returns_one()
    {
        var code = await Program.Main(["explain", "step_not_real"]);
        await Assert.That(code).IsEqualTo(1);
    }

    [Test]
    public async Task Main_unknown_command_returns_one()
    {
        var code = await Program.Main(["not-a-command"]);
        await Assert.That(code).IsEqualTo(1);
    }
}

public sealed class CodegenOutputCatalogTests
{
    [Test]
    public async Task AllGeneratedFiles_includes_bindings_and_runtime_gcs()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var files = CodegenOutputCatalog.AllGeneratedFiles(repoRoot);
        await Assert.That(files.Count).IsGreaterThan(0);
        await Assert.That(files.Any(f => f.EndsWith("Raylib6Native.g.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(files.Any(f => f.EndsWith("Graphics.g.cs", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}

public sealed class NativeShimCatalogTests
{
    [Test]
    public async Task NativeProjectDirs_lists_three_native_trees()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var dirs = NativeShimCatalog.NativeProjectDirs(repoRoot).ToList();
        await Assert.That(dirs.Count).IsEqualTo(3);
        await Assert.That(dirs.All(d => d.Contains("codegen", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }

    [Test]
    public async Task CopyMap_returns_platform_specific_artifact_names()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var map = NativeShimCatalog.CopyMap(repoRoot).ToList();
        await Assert.That(map.Count).IsEqualTo(3);
        if (OperatingSystem.IsWindows())
            await Assert.That(map[0].DestName).IsEqualTo("novolis_raylib_trace.dll");
        else if (OperatingSystem.IsLinux())
            await Assert.That(map[0].DestName).IsEqualTo("libnovolis_raylib_trace.so");
        else if (OperatingSystem.IsMacOS())
            await Assert.That(map[0].DestName).IsEqualTo("libnovolis_raylib_trace.dylib");
    }

    [Test]
    public async Task ArtifactPaths_aligns_with_copy_map_dest_names()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var artifactPaths = NativeShimCatalog.ArtifactPaths(repoRoot);
        var destNames = NativeShimCatalog.CopyMap(repoRoot).Select(p => p.DestName).ToList();
        await Assert.That(artifactPaths.Count).IsEqualTo(destNames.Count);
        foreach (var dest in destNames)
            await Assert.That(artifactPaths.Any(p => p.EndsWith(dest, StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}

public sealed class PipelineStepMetadataTests
{
    [Test]
    public async Task All_steps_expose_non_empty_metadata()
    {
        foreach (var step in PipelineStepRegistry.CreateAll())
        {
            await Assert.That(step.Id).IsNotNullOrWhiteSpace();
            await Assert.That(step.Description).IsNotNullOrWhiteSpace();
        }
    }

    [Test]
    public async Task VerifyManifestStep_declares_source_dependency()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new VerifyManifestStep();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = TextWriter.Null,
            Force = false,
        };

        await Assert.That(step.DependsOn).Contains("step_01_source");
        await Assert.That(step.InputPaths(context).Count).IsGreaterThan(0);
        await Assert.That(step.ExpectedOutputPaths(context)).IsEmpty();
    }

    [Test]
    public async Task CodegenStep_expected_outputs_match_catalog()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new CodegenStep();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = TextWriter.Null,
            Force = false,
        };

        var expected = step.ExpectedOutputPaths(context);
        var catalog = CodegenOutputCatalog.AllGeneratedFiles(repoRoot);
        await Assert.That(expected.Count).IsEqualTo(catalog.Count);
    }

    [Test]
    public async Task SourceStep_expected_outputs_include_headers()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new SourceStep();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = TextWriter.Null,
            Force = false,
        };

        var outputs = step.ExpectedOutputPaths(context);
        await Assert.That(outputs.Any(p => p.EndsWith("raylib.h", StringComparison.OrdinalIgnoreCase))).IsTrue();
        await Assert.That(outputs.Any(p => p.EndsWith("raygui.h", StringComparison.OrdinalIgnoreCase))).IsTrue();
    }
}

[NotInParallel("pipeline-repo")]
public sealed class PipelineStepExecutionTests
{
    [Test]
    public async Task VerifyManifestStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new VerifyManifestStep();
        var log = new StringWriter();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = log,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
        await Assert.That(result.Inputs.Count).IsGreaterThan(0);
        await Assert.That(log.ToString()).Contains("verify-raylib-manifest: OK");
    }

    [Test]
    public async Task VerifyDocsStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new VerifyDocsStep();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = TextWriter.Null,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
    }

    [Test]
    public async Task CodegenStep_succeeds_against_repo()
    {
        var repoRoot = PipelinePaths.FindRepoRoot();
        var step = new CodegenStep();
        var log = new StringWriter();
        var context = new PipelineContext
        {
            Layout = new RaylibPipelineLayout(repoRoot),
            Log = log,
            Force = true,
        };

        var result = await step.ExecuteAsync(context, CancellationToken.None);
        await Assert.That(result.Status).IsEqualTo(StepStatus.Succeeded);
        await Assert.That(result.Outputs.Count).IsGreaterThan(0);
    }
}
