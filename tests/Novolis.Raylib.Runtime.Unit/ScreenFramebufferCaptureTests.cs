using System.Drawing;
using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Rendering;
using Novolis.Raylib.Shell;
using Novolis.Raylib.Testing;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class ScreenFramebufferCaptureTests
{
    [Test]
    [RunOnlyIfNativeRaylib]
    public async Task TryCopyFramebufferToRgba_returns_pixels_after_draw()
    {
        RaylibTestRuntime.EnableForAssembly();

        byte[]? captured = null;
        var cts = new CancellationTokenSource();
        var thread = new Thread(() =>
        {
            try
            {
                RaylibEmbeddedShell.Run(
                    new RaylibEmbeddedOptions { Width = 128, Height = 96, TargetFps = 60 },
                    new DelegateRaylibFrameRenderer((_, _, _) =>
                    {
                        Graphics.ClearBackground(Color.CornflowerBlue);
                        Graphics.DrawRectangle(10, 10, 40, 40, Color.Goldenrod);
                    }),
                    frame =>
                    {
                        captured = frame.RgbaPixels.ToArray();
                        cts.Cancel();
                    },
                    cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when the first frame is captured.
            }
        })
        {
            IsBackground = true,
        };

        thread.Start();
        thread.Join(TimeSpan.FromSeconds(30));

        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.Any(b => b > 100)).IsTrue();
    }
}
