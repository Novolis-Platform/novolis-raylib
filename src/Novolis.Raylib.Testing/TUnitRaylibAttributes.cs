namespace Novolis.Raylib.Testing;

/// <summary>Marks tests that require native raylib; use with <see cref="NativeRaylibTestGate"/>.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RunOnlyIfNativeRaylibAttribute : Attribute
{
}

/// <summary>Skips or fails when native offscreen is unavailable (no environment variables).</summary>
public static class NativeRaylibTestGate
{
    public static bool IsAvailable => RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested();

    public static void EnsureAvailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException(
                "Native raylib offscreen is not enabled. Call RaylibTestRuntime.EnableForAssembly() or RaylibTestRuntime.EnterNativeOffscreen().");
    }
}
