using Novolis.Raylib.Game;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Game.Unit;

public sealed class SmoothedFpsTests
{
    [Test]
    public async Task Update_first_sample_sets_instant_fps()
    {
        var fps = new SmoothedFps();
        fps.Update(1f / 60f);
        await Assert.That(fps.Value).IsGreaterThan(59f);
        await Assert.That(fps.Value).IsLessThan(61f);
    }

    [Test]
    public async Task Update_ignores_zero_delta()
    {
        var fps = new SmoothedFps();
        fps.Update(0f);
        await Assert.That(fps.Value).IsEqualTo(0f);
    }

    [Test]
    public async Task Update_applies_exponential_smoothing()
    {
        var fps = new SmoothedFps { Alpha = 0.5f };
        fps.Update(1f / 60f);
        fps.Update(1f / 30f);
        await Assert.That(fps.Value).IsGreaterThan(30f);
        await Assert.That(fps.Value).IsLessThan(60f);
    }
}

public sealed class RayGameContextTests
{
    [Test]
    public async Task Run_headless_skips_game_loop_callback()
    {
        Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, "1");
        try
        {
            var invoked = false;
            var code = RayGame.Run("ctx", 640, 480, _ => invoked = true);
            await Assert.That(code).IsEqualTo(0);
            await Assert.That(invoked).IsFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, null);
        }
    }
}
