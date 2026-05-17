using Novolis.Raylib.Debug;
using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Testing.Integration;

public sealed class NativeOffscreenGateTests
{
    [Test]
    public async Task Runtime_scope_enables_harness_without_environment_variables()
    {
        Environment.SetEnvironmentVariable(RaylibDebug.OffscreenTestsEnvironmentVariable, null);
        Environment.SetEnvironmentVariable(RaylibDebug.NativeTestsEnvironmentVariable, null);
        RaylibDebug.RefreshFromEnvironment();

        await Assert.That(RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested()).IsFalse();

        using var scope = RaylibTestRuntime.EnterNativeOffscreen();
        await Assert.That(RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested()).IsTrue();
    }
}
