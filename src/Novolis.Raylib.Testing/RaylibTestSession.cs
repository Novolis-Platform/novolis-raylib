namespace Novolis.Raylib.Testing;

/// <summary>Scoped native test session via internal runtime state (no environment variables).</summary>
public sealed class RaylibTestSession : IDisposable
{
    private readonly RaylibTestRuntime.RaylibTestScope? _scope;

    /// <summary>Optionally enters native offscreen scope for the session lifetime.</summary>
    /// <param name="enableNativeOffscreen">When true, enables native offscreen for this scope.</param>
    public RaylibTestSession(bool enableNativeOffscreen = true)
    {
        if (enableNativeOffscreen)
            _scope = RaylibTestRuntime.EnterNativeOffscreen();
    }

    /// <inheritdoc />
    public void Dispose() => _scope?.Dispose();
}
