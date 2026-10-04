using System.Diagnostics;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

[NotInParallel("raylib-glfw-lock")]
public sealed class RaylibEmbeddedHostGateTests
{
    [Test]
    public async Task Create_fails_immediately_when_another_host_owns_the_process_slot()
    {
        using var hold = RaylibEmbeddedHost.OccupyProcessSlotForTests();
        var clock = Stopwatch.StartNew();
        var message = "";
        try
        {
            RaylibEmbeddedHost.Create(new RaylibEmbeddedOptions { Width = 64, Height = 64 });
        }
        catch (InvalidOperationException ex)
        {
            message = ex.Message;
        }

        await Assert.That(message).Contains("per process");
        await Assert.That(clock.Elapsed).IsLessThan(TimeSpan.FromSeconds(1));
        await Assert.That(RaylibEmbeddedHost.IsProcessHostActive).IsTrue();
    }

    [Test]
    public async Task CreateCore_releases_glfw_lock_when_initialize_throws()
    {
        var threw = false;
        try
        {
            RaylibEmbeddedHost.CreateCore(
                new RaylibEmbeddedOptions { Width = 64, Height = 64 },
                _ => throw new InvalidOperationException("init boom"));
        }
        catch (InvalidOperationException ex) when (ex.Message == "init boom")
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
        await Assert.That(RaylibEmbeddedHost.IsProcessHostActive).IsFalse();

        var acquired = RaylibGlfwProcessSync.TryEnter(TimeSpan.FromMilliseconds(400), out var scope);
        using (scope)
            await Assert.That(acquired).IsTrue();
    }
}
