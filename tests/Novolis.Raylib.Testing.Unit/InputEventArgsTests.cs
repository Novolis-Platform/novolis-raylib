using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Input;

namespace Novolis.Raylib.Testing.Unit;

public sealed class InputEventArgsTests
{
    [Test]
    public async Task MouseEventArgs_StoresPayload()
    {
        var args = new MouseEventArgs(10, 20, 1);
        await Assert.That(args.X).IsEqualTo(10);
        await Assert.That(args.Y).IsEqualTo(20);
        await Assert.That(args.Button).IsEqualTo(1);
    }

    [Test]
    public async Task KeyboardEventArgs_StoresKeyCode()
    {
        var args = new KeyboardEventArgs(32);
        await Assert.That(args.KeyCode).IsEqualTo(32);
    }
}
