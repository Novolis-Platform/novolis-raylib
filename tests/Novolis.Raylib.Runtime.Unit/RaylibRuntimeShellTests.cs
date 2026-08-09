using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public class RaylibRuntimeShellTests
{
    [Test]
    [NotInParallel("raylib-headless-env")]
    public async Task Headless_shell_skips_window()
    {
        Environment.SetEnvironmentVariable(
            RaylibRuntimeShell.HeadlessEnvironmentVariable,
            "1",
            EnvironmentVariableTarget.Process);
        try
        {
            var code = RaylibRuntimeShell.RunShellFrame("test", new NoOpRenderer());
            await Assert.That(code).IsEqualTo(0);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                RaylibRuntimeShell.HeadlessEnvironmentVariable,
                null,
                EnvironmentVariableTarget.Process);
        }
    }

    private sealed class NoOpRenderer : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) { }
    }
}
