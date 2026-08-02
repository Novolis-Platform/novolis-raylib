using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibRuntimeShellExtendedTests
{
    [Test]
    [Arguments("1")]
    [Arguments("true")]
    [Arguments("yes")]
    [Arguments("TRUE")]
    public async Task RunShellFrame_skips_window_for_headless_env(string value)
    {
        Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, value);
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
            Environment.SetEnvironmentVariable(RaylibRuntimeShell.HeadlessEnvironmentVariable, null);
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

public sealed class RaylibEmbeddedOptionsTests
{
    [Test]
    public async Task Defaults_match_hidden_host_profile()
    {
        var options = new RaylibEmbeddedOptions();
        await Assert.That(options.Width).IsEqualTo(640);
        await Assert.That(options.Height).IsEqualTo(480);
        await Assert.That(options.TargetFps).IsEqualTo(60);
        await Assert.That(options.HideWindow).IsTrue();
        await Assert.That(options.DisableExitKey).IsTrue();
        await Assert.That(options.WindowTitle).IsEqualTo("Novolis.Raylib.Embedded");
    }
}

public sealed class RaylibEmbeddedFrameTests
{
    [Test]
    public async Task Constructor_preserves_buffer_dimensions()
    {
        var pixels = new byte[] { 1, 2, 3, 4 };
        var frame = new RaylibEmbeddedFrame(pixels, 2, 1);
        await Assert.That(frame.Width).IsEqualTo(2);
        await Assert.That(frame.Height).IsEqualTo(1);
        await Assert.That(frame.RgbaPixels.Length).IsEqualTo(pixels.Length);
        await Assert.That(frame.RgbaPixels.ToArray()).IsEquivalentTo(pixels);
    }
}

public sealed class RaylibGlfwProcessSyncTests
{
    [Test]
    public async Task Enter_acquires_and_releases_mutex()
    {
        using var scope = RaylibGlfwProcessSync.Enter();
        await Assert.That(scope.GetType().Name).IsEqualTo("LockScope");
    }
}
