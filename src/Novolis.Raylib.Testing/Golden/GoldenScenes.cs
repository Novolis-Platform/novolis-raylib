using System.Drawing;
using System.Numerics;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.Testing.Golden;

/// <summary>Shared draw routines for golden tests and baseline seeding.</summary>
public static class GoldenScenes
{
    /// <summary>Draws the standard smoke-test scene (panels, diagonals, circle).</summary>
    /// <param name="deltaSeconds">Frame delta time (unused).</param>
    /// <param name="screenWidth">Framebuffer width in pixels.</param>
    /// <param name="screenHeight">Framebuffer height in pixels.</param>
    public static void DrawSmokeScene(float deltaSeconds, int screenWidth, int screenHeight)
    {
        _ = deltaSeconds;
        var panelBlue = Color.FromArgb(255, 66, 135, 245);
        var panelGreen = Color.FromArgb(255, 80, 200, 120);
        var accentRed = Color.FromArgb(255, 230, 41, 55);
        var border = Color.FromArgb(255, 40, 40, 40);

        Graphics.ClearBackground(RaylibColors.RayWhite);
        Graphics.DrawRectangleLines(4, 4, screenWidth - 8, screenHeight - 8, border);
        Graphics.DrawLine(0, 0, screenWidth, screenHeight, border);
        Graphics.DrawLine(screenWidth, 0, 0, screenHeight, border);

        var midY = screenHeight / 2;
        Graphics.DrawRectangle(16, midY - 36, 88, 72, panelBlue);
        Graphics.DrawRectangle(screenWidth - 104, midY - 36, 88, 72, panelGreen);
        Graphics.DrawCircle(screenWidth / 2, midY, 28, accentRed);
        Graphics.DrawRectangleRec(new RectangleF(16, 16, screenWidth - 32, 28), border);
        Graphics.DrawText("Novolis smoke", 24, 22, 18, RaylibColors.RayWhite);
        Graphics.DrawText("pre-Game visual check", 24, screenHeight - 36, 14, RaylibColors.DarkGray);
    }

    /// <summary>Draws a minimal HUD overlay scene for golden tests.</summary>
    /// <param name="deltaSeconds">Frame delta time (unused).</param>
    /// <param name="screenWidth">Framebuffer width in pixels.</param>
    /// <param name="screenHeight">Framebuffer height in pixels.</param>
    public static void DrawHudScene(float deltaSeconds, int screenWidth, int screenHeight)
    {
        _ = deltaSeconds;
        var darkBlue = Color.FromArgb(255, 0, 40, 120);
        var skyBlue = Color.FromArgb(255, 66, 180, 245);
        var gold = Color.FromArgb(255, 230, 190, 40);
        Graphics.ClearBackground(darkBlue);
        Hud.Clear(darkBlue);
        Hud.Text("HUD Golden", 12, 12, 24, RaylibColors.RayWhite);
        Hud.Rect(8, 40, screenWidth - 16, 4, skyBlue);
        Hud.Line(0, screenHeight - 1, screenWidth, screenHeight - 1, gold);
    }

    /// <summary>Draws a 3D cube in world space for golden tests.</summary>
    /// <param name="deltaSeconds">Frame delta time (unused).</param>
    /// <param name="screenWidth">Framebuffer width in pixels (unused).</param>
    /// <param name="screenHeight">Framebuffer height in pixels (unused).</param>
    public static void DrawWorldCube(float deltaSeconds, int screenWidth, int screenHeight)
    {
        _ = deltaSeconds;
        _ = screenWidth;
        _ = screenHeight;
        Graphics.ClearBackground(RaylibColors.RayWhite);
        var camera = Camera.Perspective(new Vector3(3f, 3f, 5f), Vector3.Zero, Vector3.UnitY);
        World.Begin(camera);
        World.DrawCubeV(Vector3.Zero, new Vector3(1.5f, 1f, 2f), Color.FromArgb(255, 230, 41, 55));
        World.DrawCubeWiresV(Vector3.Zero, new Vector3(1.5f, 1f, 2f), RaylibColors.DarkGray);
        World.End();
    }
}
