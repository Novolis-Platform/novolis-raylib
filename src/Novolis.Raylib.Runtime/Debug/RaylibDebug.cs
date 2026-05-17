using Novolis.Raylib.Audio;
using System.Drawing;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Debug;

/// <summary>
/// Process-wide raylib debug automation: environment gates (read on first use and via <see cref="RefreshFromEnvironment"/>),
/// plus programmatic toggles from <see cref="Start"/> / <see cref="Reset"/>.
/// </summary>
public static class RaylibDebug
{
    public const string OffscreenTestsEnvironmentVariable = "NOVOLIS_RAYLIB_OFFSCREEN_TESTS";

    public const string NativeTestsEnvironmentVariable = "NOVOLIS_RAYLIB_NATIVE_TESTS";

    public const string DebugHttpEnvironmentVariable = "NOVOLIS_RAYLIB_DEBUG_HTTP";

    static RaylibDebug() => RefreshFromEnvironment();

    private static bool _offscreenFromEnv;

    private static bool _nativeFromEnv;

    private static bool _captureFromEnv;

    private static bool _debugHttpFromEnv;

    public static bool OffscreenTestsRequestedFromEnvironment => _offscreenFromEnv;

    public static bool NativeTestsRequestedFromEnvironment => _nativeFromEnv;

    public static bool MemoryFramebufferCaptureRequestedFromEnvironment => _captureFromEnv;

    public static bool DebugHttpRequestedFromEnvironment => _debugHttpFromEnv;

    public static bool IsRaylibDebugAutomationActive =>
        NativeOffscreenTestHarnessEnabled
        || MemoryFramebufferCaptureEnabled
        || _offscreenFromEnv
        || _nativeFromEnv
        || _captureFromEnv;

    public static volatile bool NativeOffscreenTestHarnessEnabled;

    public static volatile bool MemoryFramebufferCaptureEnabled;

    public static void RefreshFromEnvironment()
    {
        _offscreenFromEnv = ReadEnvTruth(OffscreenTestsEnvironmentVariable);
        _nativeFromEnv = ReadEnvTruth(NativeTestsEnvironmentVariable);
        _captureFromEnv = ReadEnvTruth(RaylibDebugFrameHooks.CaptureEnvironmentVariable);
        _debugHttpFromEnv = ReadEnvTruth(DebugHttpEnvironmentVariable);
    }

    public static void Start()
    {
        NativeOffscreenTestHarnessEnabled = true;
        MemoryFramebufferCaptureEnabled = true;
        RaylibDebugCaptureGate.ProgrammaticEnabled = true;
    }

    public static void Reset()
    {
        NativeOffscreenTestHarnessEnabled = false;
        MemoryFramebufferCaptureEnabled = false;
        RaylibDebugCaptureGate.ProgrammaticEnabled = false;
        RefreshFromEnvironment();
    }

    internal static bool IsMemoryFramebufferCaptureRequested()
    {
        RefreshFromEnvironment();
        return MemoryFramebufferCaptureEnabled
               || _captureFromEnv
               || RaylibDebugCaptureGate.IsRequested(RaylibDebugFrameHooks.CaptureEnvironmentVariable);
    }

    public static LoopResult RunMinimalFrameLoop(LoopOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new LoopOptions();
        Logger.SetTraceLogLevel(TraceLogLevel.Warning);
        AudioDevice.Init();
        Window.Init(Math.Max(64, options.Width), Math.Max(64, options.Height), options.WindowTitle);
        if (!Window.IsReady())
        {
            Window.Close();
            AudioDevice.Close();
            return new LoopResult(false, options.Width, options.Height, 0);
        }

        if (options.HideWindow)
            Window.SetState(WindowStateFlags.Hidden);

        RayguiShimHost.EnsureInitialized();
        Time.SetTargetFPS(60);

        var frames = 0;
        try
        {
            while (frames < options.MaxFrames && !cancellationToken.IsCancellationRequested)
            {
                Graphics.BeginDrawing();
                Graphics.ClearBackground(Color.FromArgb(255, 20, 24, 32));
                Graphics.EndDrawing();
                frames++;
            }
        }
        finally
        {
            Window.Close();
            AudioDevice.Close();
        }

        return new LoopResult(true, options.Width, options.Height, frames);
    }

    public sealed class LoopOptions
    {
        public int Width { get; init; } = 320;

        public int Height { get; init; } = 240;

        public string WindowTitle { get; init; } = "Novolis.Raylib.InteropDebug";

        public bool HideWindow { get; init; }

        public int MaxFrames { get; init; } = 3;
    }

    public readonly record struct LoopResult(bool Ok, int Width, int Height, int FramesPresented);

    private static bool ReadEnvTruth(string name)
    {
        var v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(v))
            return false;

        return v.Equals("1", StringComparison.OrdinalIgnoreCase)
               || v.Equals("true", StringComparison.OrdinalIgnoreCase)
               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
