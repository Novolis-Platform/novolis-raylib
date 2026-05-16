using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib;

/// <summary>
/// Loads the in-box <c>novolis_raygui</c> native module (raygui built against raylib 6; see <c>native/raylib6-with-raygui</c>).
/// </summary>
public static class RayguiShimHost
{
    private const int StateUninitialized = 0;
    private const int StateReady = 1;
    private const int StateFailed = 2;

    private static readonly object Gate = new();
    private static int _state;
    private static string? _failureMessage;

    public static bool IsRayguiEnabled
    {
        get
        {
            EnsureInitialized();
            return true;
        }
    }

    public static void EnsureInitialized()
    {
        if (Volatile.Read(ref _state) == StateReady)
            return;

        lock (Gate)
        {
            if (_state == StateReady)
                return;
            if (_state == StateFailed)
                throw new InvalidOperationException(_failureMessage);

            var baseDir = AppContext.BaseDirectory;
            var raylibPath = Path.Combine(baseDir, RaylibNativeLibraryName);
            if (File.Exists(raylibPath))
                NativeLibrary.TryLoad(raylibPath, out _);

            var path = Path.Combine(baseDir, ShimLibraryName);
            if (!NativeLibrary.TryLoad(path, out var module))
            {
                _failureMessage = BuildShimLoadFailureMessage(path, baseDir);
                _state = StateFailed;
                throw new InvalidOperationException(_failureMessage);
            }

            if (!RayguiShimExports.TryBindShim(module, out var bindError))
            {
                NativeLibrary.Free(module);
                _failureMessage =
                    $"Raygui shim export binding failed ({ShimLibraryName}): {bindError}. Regenerate interop and rebuild the native shim (see pipeline/raylib6/BUILDING.txt).";
                _state = StateFailed;
                throw new InvalidOperationException(_failureMessage);
            }

            RayguiShimExports.ApplyDefaultGuiStyle();

            _state = StateReady;
        }
    }

    private static string BuildShimLoadFailureMessage(string shimPath, string baseDir)
    {
        var fileName = Path.GetFileName(shimPath);
        return
            $"Required raygui native shim was not loaded: expected '{fileName}' in the application base directory '{baseDir}'. "
            + "From the repository root run: dotnet run pipeline/raylib6/run.cs all "
            + "(or fetch-sources then: dotnet run pipeline/raylib6/run.cs native). "
            + "See pipeline/raylib6/BUILDING.txt.";
    }

    private static string ShimLibraryName =>
        OperatingSystem.IsWindows() ? "novolis_raygui.dll" :
        OperatingSystem.IsMacOS() ? "libnovolis_raygui.dylib" :
        "libnovolis_raygui.so";

    private static string RaylibNativeLibraryName =>
        OperatingSystem.IsWindows() ? "raylib.dll" :
        OperatingSystem.IsMacOS() ? "libraylib.dylib" :
        "libraylib.so";
}
