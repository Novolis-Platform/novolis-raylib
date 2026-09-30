using Novolis.Raylib.Game;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Game.Unit;

public sealed class RayGameContextTests
{
    [Test]
    [NotInParallel("raylib-headless-env")]
    public async Task Run_headless_skips_game_loop_callback()
    {
        Environment.SetEnvironmentVariable(
            RaylibRuntimeShell.HeadlessEnvironmentVariable,
            "1",
            EnvironmentVariableTarget.Process);
        try
        {
            var invoked = false;
            var code = RayGame.Run("ctx", 640, 480, _ => invoked = true);
            await Assert.That(code).IsEqualTo(0);
            await Assert.That(invoked).IsFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                RaylibRuntimeShell.HeadlessEnvironmentVariable,
                null,
                EnvironmentVariableTarget.Process);
        }
    }
}
