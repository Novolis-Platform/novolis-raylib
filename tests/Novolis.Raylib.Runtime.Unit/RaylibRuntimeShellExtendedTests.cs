using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibRuntimeShellExtendedTests
{
    [Test]
    [NotInParallel("raylib-headless-env")]
    [Arguments("1")]
    [Arguments("true")]
    [Arguments("yes")]
    [Arguments("TRUE")]
    public async Task RunShellFrame_skips_window_for_headless_env(string value)
    {
        Environment.SetEnvironmentVariable(
            RaylibRuntimeShell.HeadlessEnvironmentVariable,
            value,
            EnvironmentVariableTarget.Process);
        try
        {
            var invoked = false;
            var code = RaylibRuntimeShell.RunShellFrame(
                "headless",
                new DelegateRenderer(() => invoked = true));
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

    [Test]
    public async Task RunShellFrame_throws_when_renderer_null()
    {
        var threw = false;
        try
        {
            RaylibRuntimeShell.RunShellFrame("bad", (IRaylibFrameRenderer)null!);
        }
        catch (ArgumentNullException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Default_window_dimensions_are_hd()
    {
        var width = RaylibRuntimeShell.DefaultWindowWidth;
        var height = RaylibRuntimeShell.DefaultWindowHeight;
        await Assert.That(width).IsEqualTo(1920);
        await Assert.That(height).IsEqualTo(1080);
    }

    private sealed class DelegateRenderer(Action onFrame) : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) => onFrame();
    }
}
