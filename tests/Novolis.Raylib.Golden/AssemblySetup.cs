using Novolis.Raylib.Testing;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

/// <summary>Enables native offscreen for all golden tests without environment variables.</summary>
public static class GoldenAssemblySetup
{
    [Before(Assembly)]
    public static void EnableNativeTestRuntime() => RaylibTestRuntime.EnableForAssembly();

    [After(Assembly)]
    public static void LogLatestGoldenRunFolder()
    {
        var runFolder = GoldenRenderOutputLayout.SharedRunFolder;
        if (!string.IsNullOrWhiteSpace(runFolder))
            Console.WriteLine($"Golden QA run folder: {runFolder}");

        GoldenRenderOutputLayout.ResetSharedRun();
    }
}
