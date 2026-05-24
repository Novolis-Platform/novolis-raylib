using System.Runtime.InteropServices;

namespace Novolis.Raylib;

/// <summary>
/// Loads the in-box <c>novolis_raylib_trace</c> native module (raylib trace log forwarder; see <c>native/raylib6-platform</c>).
/// </summary>
public static class RaylibTraceHost
{
    private const int StateUninitialized = 0;
    private const int StateReady = 1;
    private const int StateFailed = 2;

    private static readonly object Gate = new();
    private static int _state;
    private static string? _failureMessage;

    /// <summary>Loads the raylib trace forwarder native module; throws if it is missing.</summary>
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

            var path = Path.Combine(baseDir, TraceLibraryName);
            if (!NativeLibrary.TryLoad(path, out _))
            {
                _failureMessage = BuildLoadFailureMessage(path, baseDir);
                _state = StateFailed;
                throw new InvalidOperationException(_failureMessage);
            }

            _state = StateReady;
        }
    }

    private static string BuildLoadFailureMessage(string shimPath, string baseDir)
    {
        var fileName = Path.GetFileName(shimPath);
        return
            $"Required raylib trace native module was not loaded: expected '{fileName}' in the application base directory '{baseDir}'. "
            + "From the repository root run: dotnet run pipeline/raylib6/run.cs all "
            + "(or fetch-sources then: dotnet run pipeline/raylib6/run.cs native). "
            + "See pipeline/raylib6/BUILDING.txt.";
    }

    private static string TraceLibraryName =>
        OperatingSystem.IsWindows() ? "novolis_raylib_trace.dll" :
        OperatingSystem.IsMacOS() ? "libnovolis_raylib_trace.dylib" :
        "libnovolis_raylib_trace.so";

    private static string RaylibNativeLibraryName =>
        OperatingSystem.IsWindows() ? "raylib.dll" :
        OperatingSystem.IsMacOS() ? "libraylib.dylib" :
        "libraylib.so";
}
