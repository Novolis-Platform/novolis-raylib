using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Testing.Unit;

public sealed class SimulatedInputTests
{
    [Test]
    public async Task Press_dequeues_in_order()
    {
        var input = new SimulatedInput();
        input.Press(KeyboardKey.Space);
        input.Press(KeyboardKey.Escape);
        await Assert.That(input.TryDequeue(out var first)).IsTrue();
        await Assert.That(first).IsEqualTo(KeyboardKey.Space);
        await Assert.That(input.TryDequeue(out var second)).IsTrue();
        await Assert.That(second).IsEqualTo(KeyboardKey.Escape);
        await Assert.That(input.TryDequeue(out _)).IsFalse();
    }

    [Test]
    public async Task Clear_empties_queue()
    {
        var input = new SimulatedInput();
        input.Press(KeyboardKey.A);
        input.Clear();
        await Assert.That(input.TryDequeue(out _)).IsFalse();
    }
}
