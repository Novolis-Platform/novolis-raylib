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
    /// <summary>Environment variable that enables offscreen integration tests (<c>1</c>, <c>true</c>, or <c>yes</c>).</summary>
    public const string OffscreenTestsEnvironmentVariable = "NOVOLIS_RAYLIB_OFFSCREEN_TESTS";

    /// <summary>Environment variable that enables native interop smoke tests.</summary>
    public const string NativeTestsEnvironmentVariable = "NOVOLIS_RAYLIB_NATIVE_TESTS";

    /// <summary>Environment variable that enables optional debug HTTP endpoints during automation.</summary>
    public const string DebugHttpEnvironmentVariable = "NOVOLIS_RAYLIB_DEBUG_HTTP";

    static RaylibDebug() => RefreshFromEnvironment();

    private static bool _offscreenFromEnv;

    private static bool _nativeFromEnv;

    private static bool _captureFromEnv;

    private static bool _debugHttpFromEnv;

    /// <summary>Whether <see cref="OffscreenTestsEnvironmentVariable"/> is set to a truthy value.</summary>
    public static bool OffscreenTestsRequestedFromEnvironment => _offscreenFromEnv;

    /// <summary>Whether <see cref="NativeTestsEnvironmentVariable"/> is set to a truthy value.</summary>
    public static bool NativeTestsRequestedFromEnvironment => _nativeFromEnv;

    /// <summary>Whether memory framebuffer capture was requested via environment.</summary>
    public static bool MemoryFramebufferCaptureRequestedFromEnvironment => _captureFromEnv;

    /// <summary>Whether <see cref="DebugHttpEnvironmentVariable"/> is set to a truthy value.</summary>
    public static bool DebugHttpRequestedFromEnvironment => _debugHttpFromEnv;

    /// <summary>True when any debug automation path (environment or programmatic) is active.</summary>
    public static bool IsRaylibDebugAutomationActive =>
        NativeOffscreenTestHarnessEnabled
        || MemoryFramebufferCaptureEnabled
        || _offscreenFromEnv
        || _nativeFromEnv
        || _captureFromEnv;

    /// <summary>Programmatically enables the native offscreen test harness.</summary>
    public static volatile bool NativeOffscreenTestHarnessEnabled;

    /// <summary>Programmatically enables CPU memory framebuffer capture hooks.</summary>
    public static volatile bool MemoryFramebufferCaptureEnabled;

    /// <summary>Re-reads debug-related environment variables into cached flags.</summary>
    public static void RefreshFromEnvironment()
    {
        _offscreenFromEnv = ReadEnvTruth(OffscreenTestsEnvironmentVariable);
        _nativeFromEnv = ReadEnvTruth(NativeTestsEnvironmentVariable);
        _captureFromEnv = ReadEnvTruth(RaylibDebugFrameHooks.CaptureEnvironmentVariable);
        _debugHttpFromEnv = ReadEnvTruth(DebugHttpEnvironmentVariable);
    }

    /// <summary>Enables programmatic debug automation (offscreen harness and capture).</summary>
    public static void Start()
    {
        NativeOffscreenTestHarnessEnabled = true;
        MemoryFramebufferCaptureEnabled = true;
        RaylibDebugCaptureGate.ProgrammaticEnabled = true;
    }

    /// <summary>Disables programmatic debug automation and refreshes environment gates.</summary>
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

    /// <summary>Runs a minimal window loop for interop smoke tests.</summary>
    /// <param name="options">Loop sizing and frame count; defaults when <see langword="null"/>.</param>
    /// <param name="cancellationToken">Stops the loop when cancelled.</param>
    /// <returns>Outcome including whether the window initialized and how many frames were presented.</returns>
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

    /// <summary>Options for <see cref="RunMinimalFrameLoop"/>.</summary>
    public sealed class LoopOptions
    {
        /// <summary>Initial window width in pixels.</summary>
        public int Width { get; init; } = 320;

        /// <summary>Initial window height in pixels.</summary>
        public int Height { get; init; } = 240;

        /// <summary>Window title shown by raylib.</summary>
        public string WindowTitle { get; init; } = "Novolis.Raylib.InteropDebug";

        /// <summary>When true, creates the window in a hidden state.</summary>
        public bool HideWindow { get; init; }

        /// <summary>Maximum frames to present before exiting.</summary>
        public int MaxFrames { get; init; } = 3;
    }

    /// <summary>Result of <see cref="RunMinimalFrameLoop"/>.</summary>
    /// <param name="Ok">Whether the window initialized successfully.</param>
    /// <param name="Width">Window width used.</param>
    /// <param name="Height">Window height used.</param>
    /// <param name="FramesPresented">Number of frames presented.</param>
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
