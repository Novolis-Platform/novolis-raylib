using Novolis.Raylib.Debug;

namespace Novolis.Raylib.Testing;

/// <summary>Scoped native test session: enables debug gates for the lifetime of the block.</summary>
public sealed class RaylibTestSession : IDisposable
{
    private readonly bool _hadOffscreen;
    private readonly bool _hadNative;

    public RaylibTestSession(bool enableOffscreen = true, bool enableNative = true)
    {
        RaylibDebug.RefreshFromEnvironment();
        _hadOffscreen = RaylibDebug.OffscreenTestsRequestedFromEnvironment;
        _hadNative = RaylibDebug.NativeTestsRequestedFromEnvironment;
        if (enableOffscreen)
            Environment.SetEnvironmentVariable(RaylibDebug.OffscreenTestsEnvironmentVariable, "1");
        if (enableNative)
            Environment.SetEnvironmentVariable(RaylibDebug.NativeTestsEnvironmentVariable, "1");
        RaylibDebug.RefreshFromEnvironment();
        RaylibDebug.Start();
    }

    public void Dispose()
    {
        RaylibDebug.Reset();
        Environment.SetEnvironmentVariable(RaylibDebug.OffscreenTestsEnvironmentVariable, _hadOffscreen ? "1" : null);
        Environment.SetEnvironmentVariable(RaylibDebug.NativeTestsEnvironmentVariable, _hadNative ? "1" : null);
        RaylibDebug.RefreshFromEnvironment();
    }
}
