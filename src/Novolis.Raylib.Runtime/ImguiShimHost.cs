using System.Runtime.InteropServices;
using Novolis.Raylib.Interop;

namespace Novolis.Raylib;

/// <summary>
/// Loads the in-box <c>novolis_imgui</c> native module (cimgui + raylib-cimgui; see <c>native/raylib6-with-imgui</c>).
/// </summary>
public static class ImguiShimHost
{
    private const int StateUninitialized = 0;
    private const int StateReady = 1;
    private const int StateFailed = 2;

    private static readonly object Gate = new();
    private static int _state;
    private static string? _failureMessage;

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

            if (!ImguiShimExports.TryBindShim(module, out var bindError))
            {
                NativeLibrary.Free(module);
                _failureMessage =
                    $"ImGui shim export binding failed ({ShimLibraryName}): {bindError}. Regenerate interop and rebuild native (see pipeline/raylib6/BUILDING.txt).";
                _state = StateFailed;
                throw new InvalidOperationException(_failureMessage);
            }

            _state = StateReady;
        }
    }

    private static string BuildShimLoadFailureMessage(string shimPath, string baseDir)
    {
        var fileName = Path.GetFileName(shimPath);
        return
            $"Required ImGui native shim was not loaded: expected '{fileName}' in '{baseDir}'. "
            + "Run: dotnet run pipeline/raylib6/run.cs all";
    }

    private static string ShimLibraryName =>
        OperatingSystem.IsWindows() ? "novolis_imgui.dll" :
        OperatingSystem.IsMacOS() ? "libnovolis_imgui.dylib" :
        "libnovolis_imgui.so";

    private static string RaylibNativeLibraryName =>
        OperatingSystem.IsWindows() ? "raylib.dll" :
        OperatingSystem.IsMacOS() ? "libraylib.dylib" :
        "libraylib.so";
}
