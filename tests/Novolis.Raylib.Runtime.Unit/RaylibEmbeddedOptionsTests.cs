using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

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
