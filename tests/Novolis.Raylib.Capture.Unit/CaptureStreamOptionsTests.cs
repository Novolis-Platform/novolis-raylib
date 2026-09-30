using Novolis.Raylib.Capture;
using Novolis.Raylib.Runtime.Presentation;

namespace Novolis.Raylib.Capture.Unit;

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
