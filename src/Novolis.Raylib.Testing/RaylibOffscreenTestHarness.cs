using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Audio;
using Novolis.Raylib.Debug;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Testing;

/// <summary>Opt-in bounded raylib loop for tests: hidden window, <see cref="IRaylibFrameRenderer"/>, optional PNG capture.</summary>
public static class RaylibOffscreenTestHarness
{
    public const string OffscreenTestsEnvironmentVariable = RaylibDebug.OffscreenTestsEnvironmentVariable;

    public const string NativeTestsEnvironmentVariable = RaylibDebug.NativeTestsEnvironmentVariable;

    public static bool IsOffscreenTestsRequested()
    {
        RaylibDebug.RefreshFromEnvironment();
        return RaylibDebug.OffscreenTestsRequestedFromEnvironment;
    }

    public static bool IsNativeTestsRequested()
    {
        RaylibDebug.RefreshFromEnvironment();
        return RaylibDebug.NativeTestsRequestedFromEnvironment;
    }

    public static bool IsNativeOffscreenRunRequested()
    {
        RaylibDebug.RefreshFromEnvironment();
        return RaylibDebug.NativeOffscreenTestHarnessEnabled
               || (RaylibDebug.OffscreenTestsRequestedFromEnvironment && RaylibDebug.NativeTestsRequestedFromEnvironment);
    }

    public static RaylibOffscreenTestRunResult Run(
        IRaylibFrameRenderer frameRenderer,
        RaylibOffscreenTestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frameRenderer);
        options ??= new RaylibOffscreenTestOptions();

        if (!IsNativeOffscreenRunRequested())
        {
            return RaylibOffscreenTestRunResult.Skipped(
                $"Enable native offscreen tests: call RaylibDebug.Start(), or set both {OffscreenTestsEnvironmentVariable}=1 and {NativeTestsEnvironmentVariable}=1.");
        }

        try
        {
            return RunCore(frameRenderer, options, cancellationToken);
        }
        catch (DllNotFoundException ex)
        {
            return RaylibOffscreenTestRunResult.Skipped($"Native library not loaded: {ex.Message}");
        }
        catch (Exception ex)
        {
            return RaylibOffscreenTestRunResult.Failed(ex.Message);
        }
    }

    private static RaylibOffscreenTestRunResult RunCore(
        IRaylibFrameRenderer frameRenderer,
        RaylibOffscreenTestOptions options,
        CancellationToken cancellationToken)
    {
        using var glfwLock = RaylibGlfwTestSync.Enter();
        Logger.SetTraceLogLevel(TraceLogLevel.Warning);
        AudioDevice.Init();
        try
        {
            var w = Math.Max(64, options.Width);
            var h = Math.Max(64, options.Height);

            if (options.HideWindow && !options.UsePostInitHideOnly)
                Window.SetConfigFlags(WindowStateFlags.Hidden);

            Window.Init(w, h, options.WindowTitle);

            if (!Window.IsReady())
            {
                Window.Close();
                return RaylibOffscreenTestRunResult.Skipped(
                    "Raylib window is not ready (GLFW or display). Check drivers and that no other test initializes GLFW in parallel.");
            }

            try
            {
                if (options.HideWindow && options.UsePostInitHideOnly)
                    Window.SetState(WindowStateFlags.Hidden);

                RayguiShimHost.EnsureInitialized();
                Time.SetTargetFPS(60);
                Window.SetExitKey((KeyboardKey)0);

                var frames = 0;
                byte[]? lastPng = null;
                while (frames < options.MaxFrames && !cancellationToken.IsCancellationRequested)
                {
                    Graphics.BeginDrawing();
                    var dt = Time.GetFrameTime();
                    var sw = Window.GetScreenWidth();
                    var sh = Window.GetScreenHeight();
                    frameRenderer.OnFrame(dt, sw, sh);
                    Graphics.EndDrawing();
                    frames++;

                    if (options.CaptureLastFramePng && frames >= options.MaxFrames)
                    {
                        if (ScreenFramebufferCapture.TryExportFramebufferToPng(out var png))
                            lastPng = png;
                    }
                }

                if (options.CaptureLastFramePng && lastPng is null)
                    FlushFramebufferAndTryCapturePng(frameRenderer, ref lastPng);

                return RaylibOffscreenTestRunResult.Ok(frames, lastPng);
            }
            finally
            {
                Window.Close();
            }
        }
        finally
        {
            AudioDevice.Close();
        }
    }

    private static void FlushFramebufferAndTryCapturePng(IRaylibFrameRenderer frameRenderer, ref byte[]? lastPng)
    {
        for (var attempt = 0; attempt < 20 && lastPng is null; attempt++)
        {
            if (attempt > 0 && attempt % 4 == 0)
                Thread.Sleep(5);

            Window.PollInputEvents();
            Graphics.BeginDrawing();
            var dt = Time.GetFrameTime();
            var sw = Window.GetScreenWidth();
            var sh = Window.GetScreenHeight();
            frameRenderer.OnFrame(dt, sw, sh);
            Graphics.EndDrawing();

            if (ScreenFramebufferCapture.TryExportFramebufferToPng(out var png))
                lastPng = png;
        }
    }
}
