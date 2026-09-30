using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class GoldenTestPollingTests
{
    [Test]
    public async Task WaitUntil_returns_when_predicate_true()
    {
        var count = 0;
        GoldenTestPolling.WaitUntil(() => ++count >= 2, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(10));
        await Assert.That(count).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task WaitUntilAsync_returns_when_predicate_true()
    {
        var ready = false;
        _ = Task.Run(async () =>
        {
            await Task.Delay(50);
            ready = true;
        });
        await GoldenTestPolling.WaitUntilAsync(() => ready, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(10));
        await Assert.That(ready).IsTrue();
    }
}
