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
