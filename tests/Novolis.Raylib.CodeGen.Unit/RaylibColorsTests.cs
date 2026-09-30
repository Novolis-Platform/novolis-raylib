using Novolis.Raylib;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Colors;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibColorsTests
{
    [Test]
    public async Task Presets_match_raylib_palette_values()
    {
        var rayWhite = RaylibColors.RayWhite;
        var darkGray = RaylibColors.DarkGray;
        await Assert.That((int)rayWhite.A).IsEqualTo(255);
        await Assert.That((int)rayWhite.R).IsEqualTo(245);
        await Assert.That((int)darkGray.G).IsEqualTo(80);
        await Assert.That(RaylibColors.White).IsEqualTo(System.Drawing.Color.White);
        await Assert.That(RaylibColors.Black).IsEqualTo(System.Drawing.Color.Black);
    }
}
