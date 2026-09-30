using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class TextureWrapperTests
{
    [Test]
    public async Task FromNative_maps_handle_fields()
    {
        var native = new Raylib6NativeTexture { Id = 99, Width = 128, Height = 64 };
        var texture = Texture.FromNative(native);
        await Assert.That(texture.Id).IsEqualTo(99u);
        await Assert.That(texture.Width).IsEqualTo(128);
        await Assert.That(texture.Height).IsEqualTo(64);
        await Assert.That(texture.IsValid).IsTrue();
    }

    [Test]
    public async Task IsValid_false_for_zero_id()
    {
        var texture = Texture.FromNative(default);
        await Assert.That(texture.IsValid).IsFalse();
    }
}
