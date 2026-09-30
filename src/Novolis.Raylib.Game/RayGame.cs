using System.Drawing;
using System.Numerics;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Interact;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Game;

/// <summary>Jam-friendly entry: open a window and draw with minimal ceremony.</summary>
public static class RayGame
{
    /// <summary>Runs a windowed game loop with an optional one-shot initializer.</summary>
    /// <param name="title">Window title.</param>
    /// <param name="width">Initial window width in pixels.</param>
    /// <param name="height">Initial window height in pixels.</param>
    /// <param name="gameLoop">Called every frame after the window is ready.</param>
    /// <returns>Process exit code from the shell (0 when headless or normal exit).</returns>
    public static int Run(string title, int width, int height, Action<RayGameContext> gameLoop) =>
        Run(title, width, height, null, gameLoop);

    /// <summary>Runs a windowed game loop with separate initialize and update callbacks.</summary>
    /// <param name="title">Window title.</param>
    /// <param name="width">Initial window width in pixels.</param>
    /// <param name="height">Initial window height in pixels.</param>
    /// <param name="initialize">Invoked once before the first update (optional).</param>
    /// <param name="update">Called every frame.</param>
    /// <returns>Process exit code from the shell.</returns>
    public static int Run(
        string title,
        int width,
        int height,
        Action<RayGameContext>? initialize,
        Action<RayGameContext> update)
    {
        ArgumentNullException.ThrowIfNull(update);
        var ctx = new RayGameContext(width, height);
        return RaylibRuntimeShell.RunShellFrame(
            title,
            width,
            height,
            new GameFrameRenderer(ctx, initialize, update),
            showFps: false);
    }

    private sealed class GameFrameRenderer(
        RayGameContext ctx,
        Action<RayGameContext>? initialize,
        Action<RayGameContext> update) : IRaylibFrameRenderer
    {
        private bool _initialized;

        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight)
        {
            ctx.SetScreen(screenWidth, screenHeight, deltaSeconds);
            if (!_initialized)
            {
                initialize?.Invoke(ctx);
                _initialized = true;
            }

            update(ctx);
        }
    }
}
