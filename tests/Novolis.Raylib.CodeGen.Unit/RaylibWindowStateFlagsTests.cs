using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibWindowStateFlagsTests
{
    [Test]
    public async Task Hidden_matches_raylib_FLAG_WINDOW_HIDDEN()
    {
        var hidden = WindowStateFlags.Hidden;
        await Assert.That(hidden).IsEqualTo(0x0000_0080u);
    }
}
