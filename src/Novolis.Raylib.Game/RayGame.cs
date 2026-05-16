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

/// <summary>Per-frame drawing and input helpers for <c>RayGame.Run</c>.</summary>
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

    /// <summary>Current drawable width in pixels (updates when the window is resized).</summary>
    public int Width => _width;

    /// <summary>Current drawable height in pixels.</summary>
    public int Height => _height;

    /// <summary>Elapsed time for the current frame in seconds.</summary>
    public float DeltaSeconds => _dt;

    internal void SetScreen(int width, int height, float dt)
    {
        _width = width;
        _height = height;
        _dt = dt;
    }

    /// <summary>Clears the framebuffer via the HUD layer.</summary>
    public void Clear(Color color) => Hud.Clear(color);

    /// <summary>Begins 3D rendering with the given camera.</summary>
    public void BeginWorld(Camera camera) => World.Begin(camera);

    /// <summary>Ends the active 3D pass.</summary>
    public void EndWorld() => World.End();

    /// <summary>Draws an axis-aligned cube (ship-style placeholder).</summary>
    public void DrawShipBox(Vector3 position, Vector3 size, Color color) =>
        World.DrawCubeV(position, size, color);

    /// <summary>Draws wireframe cube edges.</summary>
    public void DrawShipWires(Vector3 position, Vector3 size, Color color) =>
        World.DrawCubeWiresV(position, size, color);

    /// <summary>Draws a 3D line segment.</summary>
    public void DrawBolt(Vector3 from, Vector3 to, Color color) =>
        World.DrawLine(from, to, color);

    /// <summary>Draws a bolt with a midpoint sphere glow.</summary>
    public void DrawLaserBolt(Vector3 from, Vector3 to, Color color)
    {
        World.DrawLine(from, to, color);
        var dir = Vector3.Normalize(to - from);
        var mid = from + dir * (Vector3.Distance(from, to) * 0.5f);
        World.DrawSphere(mid, 0.12f, color);
    }

    /// <summary>Draws a filled sphere in world space.</summary>
    public void DrawGlowSphere(Vector3 center, float radius, Color color) =>
        World.DrawSphere(center, radius, color);

    /// <summary>Draws a wireframe sphere in world space.</summary>
    public void DrawGlowSphereWires(Vector3 center, float radius, Color color) =>
        World.DrawSphereWires(center, radius, 8, 12, color);

    /// <summary>Draws a texture region in screen space (HUD).</summary>
    public void DrawHudTexture(Texture texture, RectangleF dest, Color tint) =>
        Hud.DrawTexturePro(texture, new RectangleF(0, 0, texture.Width, texture.Height), dest, default, 0f, tint);

    /// <summary>Draws text in screen space.</summary>
    public void HudText(string text, int x, int y, int fontSize, Color color) =>
        Hud.Text(text, x, y, fontSize, color);

    /// <summary>Draws a filled screen rectangle.</summary>
    public void HudRect(int x, int y, int w, int h, Color color) =>
        Hud.Rect(x, y, w, h, color);

    /// <summary>Draws a screen-space line.</summary>
    public void HudLine(int x1, int y1, int x2, int y2, Color color) =>
        Hud.Line(x1, y1, x2, y2, color);

    /// <summary>Returns whether the key is held down this frame.</summary>
    public bool IsKeyDown(KeyboardKey key) => Input.IsKeyDown(key);

    /// <summary>Returns whether the key was pressed this frame.</summary>
    public bool IsKeyPressed(KeyboardKey key) => Input.IsKeyPressed(key);

    /// <summary>Returns whether the mouse button is held down.</summary>
    public bool IsMouseDown(MouseButton button) => Input.IsMouseButtonDown(button);

    /// <summary>Returns whether the mouse button was pressed this frame.</summary>
    public bool IsMousePressed(MouseButton button) => Input.IsMouseButtonPressed(button);

    /// <summary>Mouse movement since the last frame.</summary>
    public Vector2 MouseDelta => Input.GetMouseDelta();

    /// <summary>Hides and locks the cursor (first-person style).</summary>
    public void DisableCursor() => Input.DisableCursor();

    /// <summary>Shows the cursor.</summary>
    public void EnableCursor() => Input.EnableCursor();

    /// <summary>Alias for <see cref="HudText"/>.</summary>
    public void Text(string text, int x, int y, int fontSize, Color color) =>
        HudText(text, x, y, fontSize, color);

    /// <summary>Alias for <see cref="HudRect"/>.</summary>
    public void Rect(int x, int y, int w, int h, Color color) =>
        HudRect(x, y, w, h, color);
}
