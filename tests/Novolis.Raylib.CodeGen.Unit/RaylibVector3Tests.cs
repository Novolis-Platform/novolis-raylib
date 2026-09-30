using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibVector3Tests
{
    [Test]
    public async Task ForwardFromYawPitch_at_zero_points_down_negative_z()
    {
        var forward = RaylibVector3.ForwardFromYawPitch(0f, 0f);
        await Assert.That(forward.X).IsEqualTo(0f).Within(0.001f);
        await Assert.That(forward.Y).IsEqualTo(0f).Within(0.001f);
        await Assert.That(forward.Z).IsEqualTo(-1f).Within(0.001f);
    }

    [Test]
    public async Task ForwardFromYawPitch_returns_unit_length()
    {
        var forward = RaylibVector3.ForwardFromYawPitch(0.7f, -0.3f);
        var length = forward.Length();
        await Assert.That(length).IsEqualTo(1f).Within(0.001f);
    }
}
