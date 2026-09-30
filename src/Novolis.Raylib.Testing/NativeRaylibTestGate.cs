namespace Novolis.Raylib.Testing;

/// <summary>Reports whether native offscreen is available (runtime state or legacy env).</summary>
public static class NativeRaylibTestGate
{
    /// <summary>True when native offscreen tests may run.</summary>
    public static bool IsAvailable => RaylibOffscreenTestHarness.IsNativeOffscreenRunRequested();
}
