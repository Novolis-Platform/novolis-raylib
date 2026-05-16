using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Testing.Integration;

public class DeterministicClockTests
{
    [Test]
    public async Task Step_accumulates_time()
    {
        var clock = new DeterministicFrameClock();
        clock.SetDelta(0.5f);
        clock.Step(2);
        await Assert.That(clock.Time).IsEqualTo(1f);
    }
}
