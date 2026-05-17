namespace Novolis.Raylib.Testing;

/// <summary>Scoped native test session via internal runtime state (no environment variables).</summary>
public sealed class RaylibTestSession : IDisposable
{
    private readonly RaylibTestRuntime.RaylibTestScope? _scope;

    public RaylibTestSession(bool enableNativeOffscreen = true)
    {
        if (enableNativeOffscreen)
            _scope = RaylibTestRuntime.EnterNativeOffscreen();
    }

    public void Dispose() => _scope?.Dispose();
}
