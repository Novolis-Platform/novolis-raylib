using System.Numerics;
using Novolis.Raylib.Game;

namespace Novolis.Raylib.Game.Unit;

public sealed class RayGameContextScreenTests
{
    [Test]
    public async Task SetScreen_updates_dimensions_and_delta()
    {
        var ctx = new RayGameContext(640, 480);
        ctx.SetScreen(1024, 768, 0.05f);
        await Assert.That(ctx.Width).IsEqualTo(1024);
        await Assert.That(ctx.Height).IsEqualTo(768);
        await Assert.That(ctx.DeltaSeconds).IsEqualTo(0.05f);
    }

    [Test]
    [NotInParallel("raylib-headless-env")]
    public async Task Run_with_initialize_invokes_once_in_headless_mode()
    {
        Environment.SetEnvironmentVariable(
            Raylib.Shell.RaylibRuntimeShell.HeadlessEnvironmentVariable,
            "1",
            EnvironmentVariableTarget.Process);
        try
        {
            var initCount = 0;
            var updateCount = 0;
            var code = RayGame.Run(
                "init-test",
                640,
                480,
                _ => initCount++,
                _ => updateCount++);
            await Assert.That(code).IsEqualTo(0);
            await Assert.That(initCount).IsEqualTo(0);
            await Assert.That(updateCount).IsEqualTo(0);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                Raylib.Shell.RaylibRuntimeShell.HeadlessEnvironmentVariable,
                null,
                EnvironmentVariableTarget.Process);
        }
    }

    [Test]
    public async Task Run_throws_when_update_is_null()
    {
        var threw = false;
        try
        {
            RayGame.Run("bad", 640, 480, (Action<RayGameContext>)null!);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }
}
