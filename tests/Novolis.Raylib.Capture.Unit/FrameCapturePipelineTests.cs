using Novolis.Raylib.Capture;
using Novolis.Raylib.Runtime.Presentation;

namespace Novolis.Raylib.Capture.Unit;

/// <summary>
/// <see cref="FrameCapturePipeline"/> / <see cref="RaylibPresentationHooks"/> are process-wide statics.
/// </summary>
[NotInParallel("raylib-capture-pipeline")]
public sealed class FrameCapturePipelineTests
{
    [Test]
    public async Task Start_and_Stop_register_presentation_hooks()
    {
        var notified = false;
        try
        {
            using (RaylibCaptureRuntimeState.Enter(new CaptureStreamOptions()))
            {
                FrameCapturePipeline.Start(new CaptureStreamOptions { MaxBufferedFrames = 2 });
                await Assert.That(FrameCapturePipeline.Reader).IsNotNull();
                RaylibPresentationHooks.Register(() => notified = true, enabled: true);
                RaylibPresentationHooks.Notify();
                await Assert.That(notified).IsTrue();
            }
        }
        finally
        {
            FrameCapturePipeline.Stop();
            RaylibPresentationHooks.Register(null, enabled: false);
        }
    }

    [Test]
    public async Task FrameCaptureSession_dispose_stops_pipeline()
    {
        try
        {
            using (var session = new FrameCaptureSession(new CaptureStreamOptions { MaxBufferedFrames = 2 }))
            {
                await Assert.That(session.Reader).IsNotNull();
            }

            await Assert.That(FrameCapturePipeline.Reader).IsNull();
            await Assert.That(RaylibCaptureRuntimeState.IsStreamingActive).IsFalse();
        }
        finally
        {
            FrameCapturePipeline.Stop();
        }
    }
}
