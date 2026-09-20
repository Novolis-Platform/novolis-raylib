using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Numerics;
using Novolis.CodeGen.Bindings;
using Novolis.Raylib.Interop;
using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Manifests;

namespace Novolis.Raylib.CodeGen.Unit;

public sealed class RaylibInteropOptimizationTests
{
    [Test]
    public async Task Interop_policy_keeps_function_level_marshalling_configuration()
    {
        var interop = RaylibBindingManifestSource.Instance.GetRequired<InteropExportsFragment>(
            FragmentKind.InteropExports,
            "raylib6");
        await Assert.That(interop.Policy.SuppressGcTransitionByFunction).IsEmpty();
        await Assert.That(interop.Policy.NeverSuppressGcTransition).Contains("ExportImageToMemory");
        await Assert.That(interop.Policy.UseDisableRuntimeMarshalling).IsTrue();
        await Assert.That(interop.Policy.FacadeMethodImpl).IsEqualTo("AggressiveInlining");
    }

    [Test]
    public async Task Committed_native_interop_uses_disable_runtime_marshalling_for_strings()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");
        var native = await File.ReadAllTextAsync(Path.Combine(
            root,
            "src", "Novolis.Raylib.Bindings", "Interop",
            "Raylib6Native.g.cs"));

        await Assert.That(native).Contains("[MarshalUsing(typeof(Utf8StringMarshaller))] string text");
        await Assert.That(native).Contains("[MarshalUsing(typeof(Utf8StringMarshaller))] string fileType");
        await Assert.That(native).DoesNotContain("SuppressGCTransition = true");
    }

    [Test]
    public async Task Blittable_struct_layouts_match_raylib_expectations()
    {
        await Assert.That(Marshal.SizeOf<RaylibColor>()).IsEqualTo(4);
        await Assert.That(Marshal.SizeOf<Vector2>()).IsEqualTo(8);
        await Assert.That(Marshal.SizeOf<RectangleF>()).IsEqualTo(16);
        await Assert.That(Marshal.SizeOf<Raylib6NativeImage>()).IsEqualTo(IntPtr.Size + 16);
    }

    [Test]
    public async Task Graphics_facade_uses_aggressive_inlining_except_block_end_drawing()
    {
        var root = RepoTestPaths.TryRepositoryRoot()
                   ?? throw new InvalidOperationException("Could not resolve repository root.");
        var graphics = await File.ReadAllTextAsync(Path.Combine(
            root,
            "src",
            "Novolis.Raylib.Runtime",
            "Rendering",
            "Graphics.g.cs"));

        await Assert.That(graphics).Contains("MethodImpl(MethodImplOptions.AggressiveInlining)");
        await Assert.That(graphics).Contains("RaylibDebugFrameHooks.NotifyAfterEndDrawing()");
        await Assert.That(graphics).DoesNotContain(
            "[MethodImpl(MethodImplOptions.AggressiveInlining)]\n    public static void EndDrawing()");
    }
}
