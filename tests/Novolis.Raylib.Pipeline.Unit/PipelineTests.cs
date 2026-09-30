using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineProfilesTests
{
    [Test]
    public async Task Resolve_generate_profile_includes_verify_and_codegen()
    {
        var steps = PipelineProfiles.Resolve("generate");
        await Assert.That(steps.Count).IsEqualTo(2);
        await Assert.That(steps[0]).IsEqualTo("step_03_verify_manifest");
        await Assert.That(steps[1]).IsEqualTo("step_06_codegen");
    }

    [Test]
    public async Task Resolve_single_step_id()
    {
        var steps = PipelineProfiles.Resolve("step_08_build");
        await Assert.That(steps.Count).IsEqualTo(1);
        await Assert.That(steps[0]).IsEqualTo("step_08_build");
    }

    [Test]
    public async Task Explain_returns_description_for_known_step()
    {
        var text = PipelineProfiles.Explain("step_06_codegen");
        await Assert.That(text).IsNotNull();
        await Assert.That(text!).Contains("g.cs");
    }
}
