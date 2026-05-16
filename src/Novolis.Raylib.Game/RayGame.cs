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
    public static int Run(string title, int width, int height, Action<RayGameContext> gameLoop) =>
        Run(title, width, height, null, gameLoop);

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

/// <summary>Per-frame drawing and input helpers for <see cref="RayGame.Run"/>.</summary>
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

    public void Clear(Color color) => Hud.Clear(color);

    public void BeginWorld(Camera camera) => World.Begin(camera);

    public void EndWorld() => World.End();

    public void DrawShipBox(Vector3 position, Vector3 size, Color color) =>
        World.DrawCubeV(position, size, color);

    public void DrawShipWires(Vector3 position, Vector3 size, Color color) =>
        World.DrawCubeWiresV(position, size, color);

    public void DrawBolt(Vector3 from, Vector3 to, Color color) =>
        World.DrawLine(from, to, color);

    public void DrawLaserBolt(Vector3 from, Vector3 to, Color color)
    {
        World.DrawLine(from, to, color);
        var dir = Vector3.Normalize(to - from);
        var mid = from + dir * (Vector3.Distance(from, to) * 0.5f);
        World.DrawSphere(mid, 0.12f, color);
    }

    public void DrawGlowSphere(Vector3 center, float radius, Color color) =>
        World.DrawSphere(center, radius, color);

    public void DrawGlowSphereWires(Vector3 center, float radius, Color color) =>
        World.DrawSphereWires(center, radius, 8, 12, color);

    public void DrawHudTexture(Texture texture, RectangleF dest, Color tint) =>
        Hud.DrawTexturePro(texture, new RectangleF(0, 0, texture.Width, texture.Height), dest, default, 0f, tint);

    public void HudText(string text, int x, int y, int fontSize, Color color) =>
        Hud.Text(text, x, y, fontSize, color);

    public void HudRect(int x, int y, int w, int h, Color color) =>
        Hud.Rect(x, y, w, h, color);

    public void HudLine(int x1, int y1, int x2, int y2, Color color) =>
        Hud.Line(x1, y1, x2, y2, color);

    public bool IsKeyDown(KeyboardKey key) => Input.IsKeyDown(key);

    public bool IsKeyPressed(KeyboardKey key) => Input.IsKeyPressed(key);

    public bool IsMouseDown(MouseButton button) => Input.IsMouseButtonDown(button);

    public bool IsMousePressed(MouseButton button) => Input.IsMouseButtonPressed(button);

    public Vector2 MouseDelta => Input.GetMouseDelta();

    public void DisableCursor() => Input.DisableCursor();

    public void EnableCursor() => Input.EnableCursor();

    public void Text(string text, int x, int y, int fontSize, Color color) =>
        HudText(text, x, y, fontSize, color);

    public void Rect(int x, int y, int w, int h, Color color) =>
        HudRect(x, y, w, h, color);
}
