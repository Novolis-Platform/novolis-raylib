using Novolis.Raylib.Colors;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Transformations;

namespace Novolis.Raylib.Testing.Integration;

/// <summary>
/// Renders a simple diagnostic scene through the offscreen harness (not <c>RayGame</c>)
/// and writes PNGs to <c>artifacts/visual-captures/</c> for visual review before building Game samples.
/// </summary>
public sealed class VisualSmokeCaptureTests
{
    private static readonly Color PanelBlue = new(66, 135, 245, 255);
    private static readonly Color PanelGreen = new(80, 200, 120, 255);
    private static readonly Color AccentRed = new(230, 41, 55, 255);
    private static readonly Color Border = new(40, 40, 40, 255);

    [Test]
    public async Task Capture_smoke_scene_writes_png_for_visual_review()
    {
        using var session = new RaylibTestSession();

        var renderer = new DelegateRaylibFrameRenderer(DrawSmokeScene);
        var result = RaylibOffscreenTestHarness.Run(
            renderer,
            new RaylibOffscreenTestOptions
            {
                WindowTitle = "Novolis.VisualSmoke",
                Width = 320,
                Height = 240,
                MaxFrames = 4,
                HideWindow = true,
                CaptureLastFramePng = true,
            });

        if (!result.RanNativeLoop)
        {
            Console.WriteLine($"Skipped (native offscreen not available): {result.Message}");
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.LastFramePng).IsNotNull();

        var png = result.LastFramePng!;
        var path = VisualCaptureArtifacts.WritePng(png, "smoke-scene.png");
        var hash = FramebufferAssert.Sha256Hex(png);
        Console.WriteLine($"Visual capture: {path}");
        Console.WriteLine($"PNG SHA256: {hash}");
    }

    private static void DrawSmokeScene(float deltaSeconds, int screenWidth, int screenHeight)
    {
        _ = deltaSeconds;
        Graphics.ClearBackground(Color.RayWhite);

        Graphics.DrawRectangleLines(4, 4, screenWidth - 8, screenHeight - 8, Border);
        Graphics.DrawLine(0, 0, screenWidth, screenHeight, Border);
        Graphics.DrawLine(screenWidth, 0, 0, screenHeight, Border);

        var midY = screenHeight / 2;
        Graphics.DrawRectangle(16, midY - 36, 88, 72, PanelBlue);
        Graphics.DrawRectangle(screenWidth - 104, midY - 36, 88, 72, PanelGreen);
        Graphics.DrawCircle(screenWidth / 2, midY, 28, AccentRed);

        Graphics.DrawRectangleRec(new Rectangle(16, 16, screenWidth - 32, 28), Border);
        Graphics.DrawText("Novolis smoke", 24, 22, 18, Color.RayWhite);
        Graphics.DrawText("pre-Game visual check", 24, screenHeight - 36, 14, Color.DarkGray);
    }
}
