using Novolis.Raylib.Capture;
using Novolis.Raylib.Runtime.Presentation;

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

public sealed class CaptureStreamOptionsTests
{
    [Test]
    public async Task Defaults_match_expected_values()
    {
        var options = new CaptureStreamOptions();
        await Assert.That(options.CaptureEveryNFrames).IsEqualTo(1);
        await Assert.That(options.MaxBufferedFrames).IsEqualTo(32);
    }

    [Test]
    public async Task Custom_options_flow_into_runtime_state()
    {
        var options = new CaptureStreamOptions { CaptureEveryNFrames = 3, MaxBufferedFrames = 16 };
        using (RaylibCaptureRuntimeState.Enter(options))
        {
            await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.CaptureEveryNFrames).IsEqualTo(3);
            await Assert.That(RaylibCaptureRuntimeState.CurrentOptions!.MaxBufferedFrames).IsEqualTo(16);
        }
    }
}

[NotInParallel("raylib-capture-pipeline")]
public sealed class FrameCapturePipelineExtendedTests
{
    [Test]
    public async Task Stop_clears_reader()
    {
        try
        {
            FrameCapturePipeline.Start(new CaptureStreamOptions { MaxBufferedFrames = 2 });
            FrameCapturePipeline.Stop();
            await Assert.That(FrameCapturePipeline.Reader).IsNull();
        }
        finally
        {
            FrameCapturePipeline.Stop();
        }
    }
}
