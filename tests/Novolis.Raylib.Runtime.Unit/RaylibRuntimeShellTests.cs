using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public class RaylibRuntimeShellTests
{
    [Test]
    public async Task Headless_shell_skips_window()
    {
        Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, "1");
        try
        {
            var code = RaylibRuntimeShell.RunShellFrame("test", new NoOpRenderer());
            await Assert.That(code).IsEqualTo(0);
        }
        finally
        {
            Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, null);
        }
    }

    private sealed class NoOpRenderer : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) { }
    }
}
