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
