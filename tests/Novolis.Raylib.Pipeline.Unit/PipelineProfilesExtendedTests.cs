using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineProfilesExtendedTests
{
    [Test]
    public async Task Resolve_all_known_profiles()
    {
        foreach (var profile in new[] { "maintainer", "generate", "ci-codegen", "agent-verify" })
        {
            var steps = PipelineProfiles.Resolve(profile);
            await Assert.That(steps.Count).IsGreaterThan(0);
        }
    }

    [Test]
    public async Task Resolve_unknown_profile_throws()
    {
        var threw = false;
        try
        {
            PipelineProfiles.Resolve("not-a-profile");
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Explain_unknown_step_returns_null()
    {
        await Assert.That(PipelineProfiles.Explain("step_unknown")).IsNull();
    }
}
