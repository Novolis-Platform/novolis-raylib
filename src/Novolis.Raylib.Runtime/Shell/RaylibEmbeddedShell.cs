using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Audio;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Shell;

/// <summary>
/// Hidden-window Raylib loop for UI hosts (Avalonia, WPF bridges). Streams RGBA frames to a callback on the render thread.
/// </summary>
public static class RaylibEmbeddedShell
{
    /// <summary>
    /// Runs a hidden GLFW window loop until <paramref name="cancellationToken"/> is cancelled.
    /// <paramref name="onFrame"/> is invoked on the render thread after each presented frame.
    /// </summary>
    public static void Run(
        RaylibEmbeddedOptions options,
        IRaylibFrameRenderer frameRenderer,
        Action<RaylibEmbeddedFrame> onFrame,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(frameRenderer);
        ArgumentNullException.ThrowIfNull(onFrame);

        using var glfwLock = RaylibGlfwProcessSync.Enter();
        Logger.SetTraceLogLevel(TraceLogLevel.Warning);
        AudioDevice.Init();

        var width = System.Math.Max(64, options.Width);
        var height = System.Math.Max(64, options.Height);
        var buffer = new byte[width * height * 4];

        try
        {
            if (options.HideWindow)
            {
                Window.SetConfigFlags(WindowStateFlags.Hidden);
            }

            Window.Init(width, height, options.WindowTitle);
            if (!Window.IsReady())
            {
                throw new InvalidOperationException(
                    "Raylib window is not ready (GLFW or display). Ensure a GPU driver is available.");
            }

            try
            {
                if (options.DisableExitKey)
                {
                    Window.SetExitKey((KeyboardKey)0);
                }

                Time.SetTargetFPS(System.Math.Clamp(options.TargetFps, 1, 240));

                while (!cancellationToken.IsCancellationRequested)
                {
                    Graphics.BeginDrawing();
                    var dt = Time.GetFrameTime();
                    var sw = Window.GetScreenWidth();
                    var sh = Window.GetScreenHeight();
                    frameRenderer.OnFrame(dt, sw, sh);
                    Graphics.EndDrawing();

                    if (ScreenFramebufferCapture.TryCopyFramebufferToRgba(buffer, sw, sh))
                    {
                        onFrame(new RaylibEmbeddedFrame(buffer, sw, sh));
                    }

                    Window.PollInputEvents();
                }
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
}
