using Novolis.Raylib.Capture;
using Novolis.Raylib.Runtime.Presentation;

namespace Novolis.Raylib.Capture.Unit;

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
