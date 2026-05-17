namespace Novolis.Raylib.Testing;

/// <summary>Skips the test when native raylib offscreen is unavailable (no environment variables).</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RunOnlyIfNativeRaylibAttribute : SkipAttribute
{
    public RunOnlyIfNativeRaylibAttribute()
        : base("Native raylib offscreen not available. Call RaylibTestRuntime.EnableForAssembly() or RaylibTestRuntime.EnterNativeOffscreen().")
    {
    }

    public override Task<bool> ShouldSkip(TestRegisteredContext context) =>
        Task.FromResult(!NativeRaylibTestGate.IsAvailable);
}

/// <summary>Reports whether native offscreen is available (runtime state or legacy env).</summary>
public static class NativeRaylibTestGate
{
    public static bool IsAvailable => RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested();
}
