using System.Numerics;
using System.Runtime.InteropServices;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Interop;
using Novolis.Raylib.Rendering;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class BindingsTypeLayoutTests
{
    [Test]
    public async Task Camera_perspective_factory_sets_projection()
    {
        var cam = Camera.Perspective(new(0, 2, 0), Vector3.Zero, Vector3.UnitY, 45f);
        await Assert.That(cam.Projection).IsEqualTo(CameraProjection.Perspective);
        await Assert.That(cam.Fovy).IsEqualTo(45f);
    }

    [Test]
    public async Task RaylibColor_struct_size_is_four_bytes()
    {
        await Assert.That(Marshal.SizeOf<RaylibColor>()).IsEqualTo(4);
    }
}
