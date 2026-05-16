using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Game;

/// <summary>Jam-friendly entry: open a window and draw with minimal ceremony.</summary>
public static class RayGame
{
    public static int Run(string title, int width, int height, Action<RayGameContext> gameLoop)
    {
        ArgumentNullException.ThrowIfNull(gameLoop);
        var ctx = new RayGameContext(width, height);
        return RaylibRuntimeShell.RunShellFrame(title, width, height, new GameFrameRenderer(ctx, gameLoop));
    }

    private sealed class GameFrameRenderer(RayGameContext ctx, Action<RayGameContext> loop) : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight)
        {
            ctx.SetScreen(screenWidth, screenHeight, deltaSeconds);
            loop(ctx);
        }
    }
}

/// <summary>Per-frame drawing helpers for <see cref="RayGame.Run"/>.</summary>
public sealed class RayGameContext
{
    private int _width;
    private int _height;
    private float _dt;

    internal RayGameContext(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public int Width => _width;

    public int Height => _height;

    public float DeltaSeconds => _dt;

    internal void SetScreen(int width, int height, float dt)
    {
        _width = width;
        _height = height;
        _dt = dt;
    }

    public void Clear(Color color) => Graphics.ClearBackground(color);

    public void Text(string text, int x, int y, int fontSize, Color color) =>
        Graphics.DrawText(text, x, y, fontSize, color);

    public void Rect(int x, int y, int w, int h, Color color) =>
        Graphics.DrawRectangle(x, y, w, h, color);
}
