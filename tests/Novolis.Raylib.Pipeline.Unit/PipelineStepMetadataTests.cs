using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

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
