using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Audio;
using Novolis.Raylib.Logging;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Timing;
using Novolis.Raylib.Windowing;

namespace Novolis.Raylib.Shell;

/// <summary>
/// raylib 6 window + render loop. Use <see cref="RunShellFrame"/> with an <see cref="IRaylibFrameRenderer"/>.
/// Set <c>NOVOLIS_RAYLIB_HEADLESS</c> to skip window creation (CI or headless hosts).
/// </summary>
public static class RaylibRuntimeShell
{
    public const string HeadlessEnvironmentVariable = "NOVOLIS_RAYLIB_HEADLESS";

    public const int DefaultWindowWidth = 1920;
    public const int DefaultWindowHeight = 1080;

    public static int RunShellFrame(string windowTitle, IRaylibFrameRenderer frameRenderer, CancellationToken cancellationToken = default) =>
        RunShellFrame(windowTitle, DefaultWindowWidth, DefaultWindowHeight, frameRenderer, cancellationToken);

    public static int RunShellFrame(
        string windowTitle,
        int width,
        int height,
        IRaylibFrameRenderer frameRenderer,
        CancellationToken cancellationToken = default)
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
        RayguiShimHost.EnsureInitialized();
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
