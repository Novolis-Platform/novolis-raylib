using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Golden;

/// <summary>Enables native offscreen for all golden tests without environment variables.</summary>
public static class GoldenAssemblySetup
{
    [Before(Assembly)]
    public static void EnableNativeTestRuntime() => RaylibTestRuntime.EnableForAssembly();
}
