using Novolis.Raylib.Game;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Game.Unit;

public class RayGameTests
{
    [Test]
    public async Task Run_respects_headless_gate()
    {
        Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, "1");
        try
        {
            var code = RayGame.Run("t", 640, 480, _ => { });
            await Assert.That(code).IsEqualTo(0);
        }
        finally
        {
            Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, null);
        }
    }
}
