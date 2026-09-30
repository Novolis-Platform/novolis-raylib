using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Internal;
using Novolis.Raylib.Shell;

namespace Novolis.Raylib.Runtime.Unit;

public sealed class RaylibEmbeddedFrameTests
{
    [Test]
    public async Task Constructor_preserves_buffer_dimensions()
    {
        var pixels = new byte[] { 1, 2, 3, 4 };
        var frame = new RaylibEmbeddedFrame(pixels, 2, 1);
        await Assert.That(frame.Width).IsEqualTo(2);
        await Assert.That(frame.Height).IsEqualTo(1);
        await Assert.That(frame.RgbaPixels.Length).IsEqualTo(pixels.Length);
        await Assert.That(frame.RgbaPixels.ToArray()).IsEquivalentTo(pixels);
    }
}
