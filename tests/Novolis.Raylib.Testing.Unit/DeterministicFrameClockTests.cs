using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class DeterministicFrameClockTests
{
    [Test]
    public async Task Step_advances_time_by_delta()
    {
        var clock = new DeterministicFrameClock();
        clock.SetDelta(1f / 30f);
        var t = clock.Step(2);
        await Assert.That(t).IsEqualTo(2f / 30f);
        await Assert.That(clock.Time).IsEqualTo(2f / 30f);
    }

    [Test]
    public async Task Reset_clears_accumulated_time()
    {
        var clock = new DeterministicFrameClock();
        clock.Step();
        clock.Reset();
        await Assert.That(clock.Time).IsEqualTo(0f);
    }
}
