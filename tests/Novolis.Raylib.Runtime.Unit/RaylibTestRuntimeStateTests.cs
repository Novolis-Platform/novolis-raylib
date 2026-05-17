using Novolis.Raylib.Internal;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibTestRuntimeStateTests
{
    [Test]
    public async Task EnterNativeOffscreen_scope_enables_flag()
    {
        await Assert.That(RaylibTestRuntimeState.NativeOffscreenEnabled).IsFalse();
        using var scope = RaylibTestRuntimeState.EnterNativeOffscreen();
        await Assert.That(RaylibTestRuntimeState.NativeOffscreenEnabled).IsTrue();
    }
}
