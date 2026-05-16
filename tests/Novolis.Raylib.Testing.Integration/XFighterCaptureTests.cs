using Novolis.Raylib.Colors;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Testing;
using Novolis.Raylib.Transformations;

namespace Novolis.Raylib.Testing.Integration;

/// <summary>Captures a scripted 3D combat frame (H-Fighter silhouette) for visual review.</summary>
public sealed class XFighterCaptureTests
{
    private static readonly Color SpaceBlack = new(4, 6, 14, 255);
    private static readonly Color Hull = new(90, 95, 110, 255);
    private static readonly Color Wing = new(70, 75, 90, 255);
    private static readonly Color LaserRed = new(255, 60, 80, 255);

    [Test]
    public async Task Capture_xfighter_combat_frame_writes_png()
    {
        using var session = new RaylibTestSession();

        var renderer = new DelegateRaylibFrameRenderer((_, w, h) =>
        {
            var camera = Camera3D.Perspective(
                Vector3.Zero,
                new Vector3(0, 0, -10),
                new Vector3(0, 1, 0),
                72f);

            Graphics.ClearBackground(SpaceBlack);
            World3D.Begin(camera);

            var center = new Vector3(0, 0, -40);
            World3D.DrawCubeV(center, new Vector3(1.2f, 0.9f, 1.8f), Hull);
            World3D.DrawCubeV(center + new Vector3(2.8f, 0, 0), new Vector3(0.35f, 0.2f, 2.4f), Wing);
            World3D.DrawCubeV(center - new Vector3(2.8f, 0, 0), new Vector3(0.35f, 0.2f, 2.4f), Wing);
            World3D.DrawLine3D(new Vector3(0, 0, -2), new Vector3(0, 0, -25), LaserRed);
            World3D.DrawSphere(new Vector3(0, 0, -30), 1.5f, new Color(255, 200, 80, 255));

            World3D.End();
            Graphics.DrawText("X-Fighter capture", 24, 24, 20, Color.White);
            _ = w;
            _ = h;
        });

        var result = RaylibOffscreenTestHarness.Run(
            renderer,
            new RaylibOffscreenTestOptions
            {
                Width = 640,
                Height = 360,
                MaxFrames = 4,
                HideWindow = true,
                CaptureLastFramePng = true,
            });

        if (!result.RanNativeLoop)
        {
            Console.WriteLine($"Skipped: {result.Message}");
            return;
        }

        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(result.LastFramePng).IsNotNull();
        var path = VisualCaptureArtifacts.WritePng(result.LastFramePng!, "xfighter-combat.png");
        Console.WriteLine($"Visual capture: {path}");
    }
}
