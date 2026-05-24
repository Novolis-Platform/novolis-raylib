using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Audio;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Shell;

/// <summary>
/// raylib 6 window + render loop. Call <c>RunShellFrame</c> with an <see cref="IRaylibFrameRenderer"/>.
/// Set <c>NOVOLIS_RAYLIB_HEADLESS</c> to skip window creation (CI or headless hosts).
/// </summary>
public static class RaylibRuntimeShell
{
    /// <summary>When set to <c>1</c>, <c>true</c>, or <c>yes</c>, skips window creation (CI/headless).</summary>
    public const string HeadlessEnvironmentVariable = "NOVOLIS_RAYLIB_HEADLESS";

    /// <summary>Default initial window width for <see cref="RunShellFrame(string, IRaylibFrameRenderer, CancellationToken)"/>.</summary>
    public const int DefaultWindowWidth = 1920;

    /// <summary>Default initial window height for <see cref="RunShellFrame(string, IRaylibFrameRenderer, CancellationToken)"/>.</summary>
    public const int DefaultWindowHeight = 1080;

    /// <summary>Runs the default-sized window loop with a frame renderer.</summary>
    /// <param name="windowTitle">Window title.</param>
    /// <param name="frameRenderer">Receives <see cref="IRaylibFrameRenderer.OnFrame"/> each frame inside <c>BeginDrawing</c>/<c>EndDrawing</c>.</param>
    /// <param name="cancellationToken">Stops the loop when cancelled.</param>
    public static int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default) =>
        RunShellFrame(windowTitle, DefaultWindowWidth, DefaultWindowHeight, frameRenderer, cancellationToken);

    /// <summary>Runs a sized window loop with optional FPS overlay.</summary>
    /// <param name="windowTitle">Window title.</param>
    /// <param name="width">Initial width in pixels.</param>
    /// <param name="height">Initial height in pixels.</param>
    /// <param name="frameRenderer">Per-frame draw callback.</param>
    /// <param name="cancellationToken">Stops the loop when cancelled.</param>
    /// <param name="showFps">When true, draws an FPS counter after the renderer returns.</param>
    public static int RunShellFrame(
        string windowTitle,
        int width,
        int height,
        IRaylibFrameRenderer frameRenderer,
        CancellationToken cancellationToken = default,
        bool showFps = true)
    {
        ArgumentNullException.ThrowIfNull(frameRenderer);

        if (IsHeadlessRequested())
        {
            Console.WriteLine($"{windowTitle}: Raylib window skipped ({HeadlessEnvironmentVariable}=1).");
            return 0;
        }

        Logger.SetTraceLogLevel(TraceLogLevel.Warning);
        AudioDevice.Init();
        Window.SetConfigFlags(WindowStateFlags.DefaultGameHost);
        Window.Init(Math.Max(64, width), Math.Max(64, height), windowTitle);
        Window.SetState(WindowStateFlags.Resizable);
        Time.SetTargetFPS(60);

        try
        {
            while (!Window.ShouldClose() && !cancellationToken.IsCancellationRequested)
            {
                var dt = Time.GetFrameTime();
                var w = Window.GetScreenWidth();
                var h = Window.GetScreenHeight();
                Graphics.BeginDrawing();
                frameRenderer.OnFrame(dt, w, h);
                if (showFps)
                    Graphics.DrawFPS(8, 8);
                Graphics.EndDrawing();
            }
        }
        finally
        {
            Window.Close();
            AudioDevice.Close();
        }

        return 0;
    }

    private static bool IsHeadlessRequested()
    {
        var v = Environment.GetEnvironmentVariable(HeadlessEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(v))
            return false;

        return v.Equals("1", StringComparison.OrdinalIgnoreCase)
               || v.Equals("true", StringComparison.OrdinalIgnoreCase)
               || v.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }
}
