using Novolis.Raylib.Runtime.Presentation;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibPresentationHooksTests
{
    [Test]
    public async Task Notify_skips_when_disabled()
    {
        var count = 0;
        RaylibPresentationHooks.Register(() => count++, enabled: false);
        RaylibPresentationHooks.Notify();
        await Assert.That(count).IsEqualTo(0);
        RaylibPresentationHooks.Register(null, enabled: false);
    }

    [Test]
    public async Task Notify_invokes_registered_handler_when_enabled()
    {
        var count = 0;
        RaylibPresentationHooks.Register(() => count++, enabled: true);
        RaylibPresentationHooks.Notify();
        await Assert.That(count).IsEqualTo(1);
        RaylibPresentationHooks.Register(null, enabled: false);
    }

    [Test]
    public async Task Register_null_handler_disables_notify()
    {
        RaylibPresentationHooks.Register(null, enabled: true);
        RaylibPresentationHooks.Notify();
        RaylibPresentationHooks.Register(null, enabled: false);
        await Task.CompletedTask;
    }
}
