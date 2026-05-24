using Novolis.Raylib.Capture;
using Novolis.Raylib.Presentation;

namespace Novolis.Raylib.Capture.Unit;

public sealed class RaylibCaptureRuntimeStateTests
{
    [Test]
    public async Task Enter_scope_sets_streaming_active()
    {
        await Assert.That(RaylibCaptureRuntimeState.IsStreamingActive).IsFalse();
        using (RaylibCaptureRuntimeState.Enter(new CaptureStreamOptions()))
        {
            await Assert.That(RaylibCaptureRuntimeState.IsStreamingActive).IsTrue();
            await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.MaxBufferedFrames).IsEqualTo(32);
        }

        await Assert.That(RaylibCaptureRuntimeState.IsStreamingActive).IsFalse();
    }

    [Test]
    public async Task Nested_scopes_restore_previous()
    {
        var outer = new CaptureStreamOptions { MaxBufferedFrames = 8 };
        var inner = new CaptureStreamOptions { MaxBufferedFrames = 4 };
        using (RaylibCaptureRuntimeState.Enter(outer))
        {
            await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.MaxBufferedFrames).IsEqualTo(8);
            using (RaylibCaptureRuntimeState.Enter(inner))
            {
                await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.MaxBufferedFrames).IsEqualTo(4);
            }

            await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.MaxBufferedFrames).IsEqualTo(8);
        }
    }
}

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
        using (var session = new FrameCaptureSession(new CaptureStreamOptions { MaxBufferedFrames = 2 }))
        {
            await Assert.That(session.Reader).IsNotNull();
        }

        await Assert.That(FrameCapturePipeline.Reader).IsNull();
        await Assert.That(RaylibCaptureRuntimeState.IsStreamingActive).IsFalse();
    }
}

public sealed class CaptureStreamOptionsTests
{
    [Test]
    public async Task Defaults_match_expected_values()
    {
        var options = new CaptureStreamOptions();
        await Assert.That(options.CaptureEveryNFrames).IsEqualTo(1);
        await Assert.That(options.MaxBufferedFrames).IsEqualTo(32);
    }
}
